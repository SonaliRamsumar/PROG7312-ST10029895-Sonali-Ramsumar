
using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class PredictiveActionService
    {
        private readonly UserActivityService _activityService;

        public PredictiveActionService(UserActivityService activityService)
        {
            _activityService = activityService;
        }

        public List<PredictiveAction> GetPredictions()
        {
            // Retrieve the recorded searches and commands.
            var activities = _activityService.GetAll();

            var predictions = new List<PredictiveAction>();

            // Find meaningful searches that can be used as triggers.
            var searches = activities
                .Where(activity =>
                    string.Equals(
                        activity.ActivityType,
                        "Search",
                        StringComparison.OrdinalIgnoreCase))
                .Where(activity =>
                    !string.IsNullOrWhiteSpace(activity.Target)
                    || !IsAll(activity.Category)
                    || !IsAll(activity.StateFilter))
                .ToList();

            // Group searches with the same query and filter combination.
            var triggerGroups = searches.GroupBy(
                activity => CreateTriggerKey(activity),
                StringComparer.OrdinalIgnoreCase);

            foreach (var triggerGroup in triggerGroups)
            {
                var triggerSearches = triggerGroup.ToList();

                // Every search in this group represents one occasion
                // on which the user could have performed a command.
                int totalTriggerCount = triggerSearches.Count;

                // Store the IDs of the searches in this trigger group.
                var triggerIds = triggerSearches
                    .Select(activity => activity.Id)
                    .ToHashSet();

                // Find successful commands linked to these searches.
                var linkedCommands = activities
                    .Where(activity =>
                        string.Equals(
                            activity.ActivityType,
                            "Command",
                            StringComparison.OrdinalIgnoreCase))
                    .Where(activity =>
                        activity.TriggerActivityId.HasValue
                        && triggerIds.Contains(
                            activity.TriggerActivityId.Value))
                    .Where(activity =>
                        string.Equals(
                            activity.ActionName,
                            "SetRelayState",
                            StringComparison.OrdinalIgnoreCase))
                    .Where(activity =>
                        !string.IsNullOrWhiteSpace(activity.Target))
                    .Where(activity =>
                        IsValidRelayState(activity.Value))
                    .ToList();

                if (linkedCommands.Count == 0)
                {
                    // No action has been associated with this trigger yet.
                    // It remains part of the trigger history but cannot
                    // produce an action suggestion.
                    continue;
                }

                // Group commands by device and suggested relay state.
                var actionGroups = linkedCommands.GroupBy(
                    activity => string.Join(
                        "|",
                        activity.Target.Trim().ToUpperInvariant(),
                        activity.ActionName.Trim().ToUpperInvariant(),
                        activity.Value.Trim().ToUpperInvariant()),
                    StringComparer.OrdinalIgnoreCase);

                var latestSearch = triggerSearches
                    .OrderByDescending(activity => activity.Timestamp)
                    .First();

                string triggerDescription =
                    DescribeTrigger(latestSearch);

                foreach (var actionGroup in actionGroups)
                {
                    var matchingCommands = actionGroup.ToList();

                    int successfulActionCount = matchingCommands.Count;

                    // Confidence is the percentage of matching search
                    // occasions followed by this particular command.
                    int confidence = (int)Math.Round(
                        successfulActionCount * 100.0 / totalTriggerCount,
                        MidpointRounding.AwayFromZero);

                    var latestCommand = matchingCommands
                        .OrderByDescending(activity => activity.Timestamp)
                        .First();

                    predictions.Add(new PredictiveAction
                    {
                        Type = "Command",

                        Title =
                            $"Set {latestCommand.Target} to {latestCommand.Value}",

                        Description =
                            $"After {triggerDescription}, this command was " +
                            $"executed {successfulActionCount} out of " +
                            $"{totalTriggerCount} matching search occasion(s).",

                        DeviceIdentifier = latestCommand.Target,
                        CommandName = latestCommand.ActionName,
                        SuggestedValue = latestCommand.Value,

                        Query = latestSearch.Target,
                        Category = latestSearch.Category,
                        StateFilter = latestSearch.StateFilter,

                        // Number of times this particular action followed
                        // the matching trigger.
                        EvidenceCount = successfulActionCount,

                        Confidence = confidence,
                        LastObserved = latestCommand.Timestamp
                    });
                }
            }

            // Display the strongest learned relationships first.
            return predictions
                .OrderByDescending(prediction => prediction.Confidence)
                .ThenByDescending(prediction => prediction.EvidenceCount)
                .ThenByDescending(prediction => prediction.LastObserved)
                .Take(4)
                .ToList();
        }

        // Creates a consistent key for matching search occasions.
        private static string CreateTriggerKey(UserActivity activity)
        {
            return string.Join(
                "|",
                Normalize(activity.Target),
                NormalizeFilter(activity.Category),
                NormalizeFilter(activity.StateFilter));
        }

        // Describes the search pattern that triggered the prediction.
        private static string DescribeTrigger(UserActivity activity)
        {
            var details = new List<string>();

            if (!string.IsNullOrWhiteSpace(activity.Target))
            {
                details.Add($"search for \"{activity.Target.Trim()}\"");
            }

            if (!IsAll(activity.Category))
            {
                details.Add($"category {activity.Category.Trim()}");
            }

            if (!IsAll(activity.StateFilter))
            {
                details.Add($"relay state {activity.StateFilter.Trim()}");
            }

            return details.Count > 0
                ? string.Join(", ", details)
                : "the recorded search";
        }

        private static bool IsValidRelayState(string? value)
        {
            return string.Equals(
                       value,
                       "On",
                       StringComparison.OrdinalIgnoreCase)
                   || string.Equals(
                       value,
                       "Off",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAll(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                || string.Equals(
                    value.Trim(),
                    "All",
                    StringComparison.OrdinalIgnoreCase);
        }

        private static string Normalize(string? value)
        {
            return value?.Trim() ?? string.Empty;
        }

        private static string NormalizeFilter(string? value)
        {
            return IsAll(value) ? "All" : value!.Trim();
        }
    }
}
