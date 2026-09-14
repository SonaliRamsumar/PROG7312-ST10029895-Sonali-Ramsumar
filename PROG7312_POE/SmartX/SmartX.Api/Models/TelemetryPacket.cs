namespace SmartX.Api.Models
{
    public class TelemetryPacket<T>
    {
        public string DeviceIdentifier { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        public T Value { get; set; } = default!;
    }
}