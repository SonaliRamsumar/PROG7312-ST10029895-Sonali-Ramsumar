
namespace SmartX.Api.Models
{
    public class TelemetryPacket<T> where T : struct
    {
        public string DeviceIdentifier { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        public T Value { get; set; } = default!;
    }
}
