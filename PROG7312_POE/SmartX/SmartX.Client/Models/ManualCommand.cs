
namespace SmartX.Client.Models
{
    public class ManualCommand
    {
        public string DeviceIdentifier { get; set; } = string.Empty;
        public string CommandName { get; set; } = string.Empty;
        public string PreviousValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
