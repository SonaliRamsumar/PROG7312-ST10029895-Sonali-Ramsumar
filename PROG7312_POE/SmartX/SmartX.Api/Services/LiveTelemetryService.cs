
using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class LiveTelemetryService
    {
        private const int MaximumReadings = 100;

        private readonly Queue<TelemetryPacket<double>> _readings = new();
        private readonly object _lock = new();

        // Store each reading so the dashboard can identify its source device.
        public void AddReading(TelemetryPacket<double> packet)
        {
            lock (_lock)
            {
                _readings.Enqueue(packet);

                // Keep memory usage predictable by removing the oldest readings.
                while (_readings.Count > MaximumReadings)
                {
                    _readings.Dequeue();
                }
            }
        }

        // Return readings from oldest to newest without removing them.
        public List<TelemetryPacket<double>> GetRecentReadings()
        {
            lock (_lock)
            {
                return _readings
                    .OrderBy(reading => reading.Timestamp)
                    .ToList();
            }
        }
    }
}
