
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
            var activities = _activityService.GetAll();
            var predictions = new List<PredictiveAction>();

            // Learn from recorded relay commands, excluding undo actions.
            var commands = activities
                .Where(activity =>
                    string.Equals(
                        activity.ActivityType,
                        "Command",
                        StringComparison.OrdinalIgnoreCase)
                    && string.Equals(
                        activity.ActionName,
                        "SetRelayState",
                        StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(activity.Target)
                    && (string.Equals(
                            activity.Value,
                            "On",
                            StringComparison.OrdinalIgnoreCase)
                        || string.Equals(
                            activity.Value,
                            "Off",
                            StringComparison.OrdinalIgnoreCase)))
                .ToList();

            // Find the most frequently used state for each device.
            foreach (var deviceGroup in commands.GroupBy(
                activity => activity.Target,
                StringComparer.OrdinalIgnoreCase))
            {
                var preferred = deviceGroup
                    .GroupBy(
                        activity => activity.Value,
                        StringComparer.OrdinalIgnoreCase)
                    .Select(group => new
                    {
                        Value = group.Key,
                        Count = group.Count(),
                        Latest = group
                            .OrderByDescending(item => item.Timestamp)
                            .First()
                    })
                    .OrderByDescending(item => item.Count)
                    .ThenByDescending(item => item.Latest.Timestamp)
                    .FirstOrDefault();

                if (preferred == null)
                {
                    continue;
                }

                predictions.Add(new PredictiveAction
                {
                    Type = "Command",
                    Title = $"Set {deviceGroup.Key} to {preferred.Value}",
                    Description =
                        $"This relay state was used {preferred.Count} time(s) for this device.",
                    DeviceIdentifier = deviceGroup.Key,
                    CommandName = "SetRelayState",
                    SuggestedValue = preferred.Value,
                    EvidenceCount = preferred.Count,
                    Confidence = CalculateConfidence(preferred.Count),
                    LastObserved = preferred.Latest.Timestamp
                });
            }

            // Learn from searches with a query or active filter.
            var searches = activities
                .Where(activity => string.Equals(
                    activity.ActivityType,
                    "Search",
                    StringComparison.OrdinalIgnoreCase))
                .Where(activity =>
                    !string.IsNullOrWhiteSpace(activity.Target)
                    || !IsAll(activity.Category)
                    || !IsAll(activity.StateFilter))
                .ToList();

            // Group searches with matching query, category and relay state.
            var searchGroups = searches.GroupBy(
                activity => string.Join(
                    "|",
                    Normalize(activity.Target),
                    NormalizeFilter(activity.Category),
                    NormalizeFilter(activity.StateFilter)),
                StringComparer.OrdinalIgnoreCase);

            foreach (var group in searchGroups)
            {
                var latest = group
                    .OrderByDescending(activity => activity.Timestamp)
                    .First();

                var query = Normalize(latest.Target);
                var category = NormalizeFilter(latest.Category);
                var state = NormalizeFilter(latest.StateFilter);

                var details = new List<string>();

                if (!string.IsNullOrWhiteSpace(query))
                {
                    details.Add($"search for \"{query}\"");
                }

                if (!IsAll(category))
                {
                    details.Add($"category {category}");
                }

                if (!IsAll(state))
                {
                    details.Add($"relay state {state}");
                }

                predictions.Add(new PredictiveAction
                {
                    Type = "Search",
                    Title = "Reuse a previous device search",
                    Description =
                        $"You used this search combination {group.Count()} time(s): " +
                        string.Join(", ", details) + ".",
                    Query = query,
                    Category = category,
                    StateFilter = state,
                    EvidenceCount = group.Count(),
                    Confidence = CalculateConfidence(group.Count()),
                    LastObserved = latest.Timestamp
                });
            }

            // Display the strongest recorded patterns first.
            return predictions
                .OrderByDescending(item => item.EvidenceCount)
                .ThenByDescending(item => item.LastObserved)
                .Take(4)
                .ToList();
        }

        private static int CalculateConfidence(int count)
        {
            // This is a simple heuristic score, not a probability.
            return Math.Min(95, 50 + Math.Max(0, count - 1) * 10);
        }

        private static bool IsAll(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                || string.Equals(
                    value,
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
