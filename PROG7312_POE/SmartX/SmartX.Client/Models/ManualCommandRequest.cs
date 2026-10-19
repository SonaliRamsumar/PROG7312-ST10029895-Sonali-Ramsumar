
namespace SmartX.Client.Models
{
    public class ManualCommandRequest
    {
        public string DeviceIdentifier { get; set; } = string.Empty;
        public string CommandName { get; set; } = "SetRelayState";
        public string NewValue { get; set; } = "Off";
    }
}
