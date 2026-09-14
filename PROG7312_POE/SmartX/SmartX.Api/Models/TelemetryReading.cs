namespace SmartX.Api.Models
{
    public class TelemetryReading
    {
        public string DeviceIdentifier { get; set; } = string.Empty;

        public double Value { get; set; }

        public TelemetryReading()
        {
        }

        public TelemetryReading(string deviceIdentifier, double value)
        {
            DeviceIdentifier = deviceIdentifier;
            Value = value;
        }

        public static TelemetryReading operator +(
            TelemetryReading reading1,
            TelemetryReading reading2)
        {
            return new TelemetryReading(
                reading1.DeviceIdentifier,
                reading1.Value + reading2.Value
            );
        }
    }
}