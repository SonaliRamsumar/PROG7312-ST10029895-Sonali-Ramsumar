
namespace SmartX.Api.Models
{
    public class UserActivity
    {
        public int Id { get; set; }

        // Identifies whether the activity was a search, command, or undo.
        public string ActivityType { get; set; } = string.Empty;

        // The device identifier or search query associated with the activity.
        public string Target { get; set; } = string.Empty;

        // The action performed, such as SetRelayState or SearchDevices.
        public string ActionName { get; set; } = string.Empty;

        // The command value, such as On or Off.
        public string Value { get; set; } = string.Empty;

        // Search filters help the engine recognise repeated searches.
        public string Category { get; set; } = string.Empty;

        public string StateFilter { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
