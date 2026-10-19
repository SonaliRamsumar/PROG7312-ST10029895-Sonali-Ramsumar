
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

        // Add two telemetry values and return a new reading.
        public static TelemetryReading operator +(
            TelemetryReading reading1,
            TelemetryReading reading2)
        {
            return new TelemetryReading(
                reading1.DeviceIdentifier,
                reading1.Value + reading2.Value
            );
        }

        // Subtract the second telemetry value from the first.
        public static TelemetryReading operator -(
            TelemetryReading reading1,
            TelemetryReading reading2)
        {
            return new TelemetryReading(
                reading1.DeviceIdentifier,
                reading1.Value - reading2.Value
            );
        }

        // Check whether the first telemetry value is greater.
        public static bool operator >(
            TelemetryReading reading1,
            TelemetryReading reading2)
        {
            return reading1.Value > reading2.Value;
        }

        // Check whether the first telemetry value is smaller.
        public static bool operator <(
            TelemetryReading reading1,
            TelemetryReading reading2)
        {
            return reading1.Value < reading2.Value;
        }
    }
}
