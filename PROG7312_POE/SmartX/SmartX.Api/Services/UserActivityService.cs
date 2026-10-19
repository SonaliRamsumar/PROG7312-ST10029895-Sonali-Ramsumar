
using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class UserActivityService
    {
        private readonly List<UserActivity> _activities = new();
        private readonly object _lock = new();
        private int _nextId = 1;

        // Record a meaningful search or filter combination.
        public void RecordSearch(
            string query,
            string category,
            string stateFilter)
        {
            query = query.Trim();
            category = string.IsNullOrWhiteSpace(category)
                ? "All"
                : category.Trim();
            stateFilter = string.IsNullOrWhiteSpace(stateFilter)
                ? "All"
                : stateFilter.Trim();

            // Ignore a search that has no query and no active filters.
            if (string.IsNullOrWhiteSpace(query)
                && category.Equals("All", StringComparison.OrdinalIgnoreCase)
                && stateFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
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

        // Record a command after it has been executed successfully.
        public void RecordCommand(
            string deviceIdentifier,
            string commandName,
            string value)
        {
            AddActivity(new UserActivity
            {
                ActivityType = "Command",
                Target = deviceIdentifier,
                ActionName = commandName,
                Value = value
            });
        }

        // Record an undo action separately from the original command.
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

        // Return the recorded activities, newest first.
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
                        Timestamp = activity.Timestamp
                    })
                    .ToArray();
            }
        }

        // Keep activity IDs and writes consistent if requests arrive together.
        private void AddActivity(UserActivity activity)
        {
            lock (_lock)
            {
                activity.Id = _nextId++;
                activity.Timestamp = DateTime.UtcNow;
                _activities.Add(activity);
            }
        }
    }
}
