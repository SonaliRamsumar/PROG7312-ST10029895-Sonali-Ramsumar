namespace SmartX.Api.Models
{
    public class ManualCommand
    {
        public string DeviceIdentifier { get; set; } = string.Empty;

        public string CommandName { get; set; } = string.Empty;

        // These values allow us to restore the previous setting.
        public string PreviousValue { get; set; } = string.Empty;

        public string NewValue { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}