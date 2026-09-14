using SmartX.Api.Models;
using System.Linq;

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

        public List<double> ConvertReadingsToList(double[] readings)
        {
            // Converts the fixed array into a List so it is easier to work with.
            return readings.ToList();
        }

        public double GetAverageReading(double[] readings)
        {
            // Works out the average value from the telemetry readings.
            if (readings.Length == 0)
            {
                return 0;
            }

            return readings.Average();
        }
    }
}