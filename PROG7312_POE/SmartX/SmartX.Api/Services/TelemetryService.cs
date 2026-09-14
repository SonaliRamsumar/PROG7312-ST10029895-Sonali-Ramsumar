using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class TelemetryService
    {
        public TelemetryPacket<T> CreatePacket<T>(
            string deviceIdentifier,
            T value)
        {
            // T keeps the original data type, such as double, int or bool.
            return new TelemetryPacket<T>
            {
                DeviceIdentifier = deviceIdentifier,
                Timestamp = DateTime.UtcNow,
                Value = value
            };
        }

        public TelemetryReading CombineReadings(
            TelemetryReading firstReading,
            TelemetryReading secondReading)
        {
            // Uses the + operator that we created in TelemetryReading.
            return firstReading + secondReading;
        }
    }
}