
using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class UserActivityService
    {
        private readonly List<UserActivity> _activities = new();

        // Protects activity records when multiple API requests arrive together.
        private readonly object _lock = new();

        private int _nextId = 1;

        // A command must follow a search within this time window
        // to be considered related to that search.
        private static readonly TimeSpan TriggerTimeWindow =
            TimeSpan.FromMinutes(10);

        // Record a meaningful search or filter combination.
        public void RecordSearch(
            string query,
            string category,
            string stateFilter)
        {
            query = query?.Trim() ?? string.Empty;

            category = string.IsNullOrWhiteSpace(category)
                ? "All"
                : category.Trim();

            stateFilter = string.IsNullOrWhiteSpace(stateFilter)
                ? "All"
                : stateFilter.Trim();

            // Ignore searches with no query and no active filters.
            if (string.IsNullOrWhiteSpace(query)
                && IsAll(category)
                && IsAll(stateFilter))
            {
                return;
            }

            AddActivity(new UserActivity
            {
                ActivityType = "Search",
                Target = query,
                ActionName = "SearchDevices",
                Category = category,
                StateFilter = stateFilter
            });
        }

        // Record a successfully executed command and link it to
        // the most recent search that has not already triggered a command.
        public void RecordCommand(
            string deviceIdentifier,
            string commandName,
            string value)
        {
            lock (_lock)
            {
                DateTime now = DateTime.UtcNow;

                // Find the latest meaningful search within the time window.
                // A search can only be paired with one ordinary command.
                var trigger = _activities
                    .Where(activity =>
                        string.Equals(
                            activity.ActivityType,
                            "Search",
                            StringComparison.OrdinalIgnoreCase))
                    .Where(activity =>
                        now >= activity.Timestamp
                        && now - activity.Timestamp <= TriggerTimeWindow)
                    .Where(activity =>
                        !_activities.Any(existing =>
                            existing.TriggerActivityId == activity.Id
                            && string.Equals(
                                existing.ActivityType,
                                "Command",
                                StringComparison.OrdinalIgnoreCase)))
                    .OrderByDescending(activity => activity.Timestamp)
                    .FirstOrDefault();

                var commandActivity = new UserActivity
                {
                    ActivityType = "Command",
                    Target = deviceIdentifier.Trim(),
                    ActionName = commandName.Trim(),
                    Value = value.Trim(),

                    // Connect the command to its search trigger.
                    TriggerActivityId = trigger?.Id
                };

                AddActivityLocked(commandActivity);
            }
        }

        // Record undo separately so it does not distort predictions.
        public void RecordUndo(
            string deviceIdentifier,
            string commandName,
            string restoredValue)
        {
            AddActivity(new UserActivity
            {
                ActivityType = "Undo",
                Target = deviceIdentifier,
                ActionName = commandName,
                Value = restoredValue
            });
        }

        // Return recorded activities, newest first.
        public UserActivity[] GetAll()
        {
            lock (_lock)
            {
                return _activities
                    .OrderByDescending(activity => activity.Timestamp)
                    .Select(activity => new UserActivity
                    {
                        Id = activity.Id,
                        ActivityType = activity.ActivityType,
                        Target = activity.Target,
                        ActionName = activity.ActionName,
                        Value = activity.Value,
                        Category = activity.Category,
                        StateFilter = activity.StateFilter,
                        TriggerActivityId = activity.TriggerActivityId,
                        Timestamp = activity.Timestamp
                    })
                    .ToArray();
            }
        }

        // Add an activity while safely managing its unique ID and timestamp.
        private void AddActivity(UserActivity activity)
        {
            lock (_lock)
            {
                AddActivityLocked(activity);
            }
        }

        // This method is called while the activity lock is already held.
        private void AddActivityLocked(UserActivity activity)
        {
            activity.Id = _nextId++;
            activity.Timestamp = DateTime.UtcNow;

            _activities.Add(activity);
        }

        private static bool IsAll(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                || string.Equals(
                    value.Trim(),
                    "All",
                    StringComparison.OrdinalIgnoreCase);
        }
    }
}
