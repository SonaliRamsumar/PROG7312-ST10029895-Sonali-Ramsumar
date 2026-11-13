
using SmartX.Api.Models;
using System.Linq;

namespace SmartX.Api.Services
{
    public class TelemetryService
    {
        // Creates a telemetry packet containing the device,
        // timestamp and value received from a sensor.
        public TelemetryPacket<T> CreatePacket<T>(
            string deviceIdentifier,
            T value) where T : struct
        {
            return new TelemetryPacket<T>
            {
                DeviceIdentifier = deviceIdentifier,
                Timestamp = DateTime.UtcNow,
                Value = value
            };
        }

        // Combines two telemetry readings using the overloaded + operator.
        public TelemetryReading CombineReadings(
            TelemetryReading firstReading,
            TelemetryReading secondReading)
        {
            return firstReading + secondReading;
        }

        // Converts an array of readings into a List.
        public List<double> ConvertReadingsToList(double[] readings)
        {
            return readings.ToList();
        }

        // Calculates the average of a collection of readings.
        public double GetAverageReading(double[] readings)
        {
            if (readings.Length == 0)
            {
                return 0;
            }

            return readings.Average();
        }
    }
}
