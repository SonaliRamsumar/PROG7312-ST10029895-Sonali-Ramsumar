namespace SmartX.Api.Models
{
    public class TelemetryAlert
    {
        public string DeviceIdentifier { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Severity { get; set; } = "Warning";

        public double Value { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}