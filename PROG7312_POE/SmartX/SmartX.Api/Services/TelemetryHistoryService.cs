
namespace SmartX.Api.Services
{
    public class TelemetryHistoryService
    {
        // Stores readings in timestamp order, with multiple readings
        // allowed to share the same timestamp.
        private readonly SortedDictionary<DateTime, List<double>> _history = new();

        public int Count => _history.Values.Sum(readings => readings.Count);

        public void AddReading(DateTime timestamp, double value)
        {
            // Create a list if this timestamp has not been recorded yet.
            if (!_history.TryGetValue(timestamp, out var readings))
            {
                readings = new List<double>();
                _history.Add(timestamp, readings);
            }

            readings.Add(value);
        }

        public List<(DateTime Timestamp, double Value)> GetReadingsInChronologicalOrder()
        {
            var result = new List<(DateTime Timestamp, double Value)>();

            // SortedDictionary automatically returns keys in ascending order.
            foreach (var entry in _history)
            {
                foreach (var value in entry.Value)
                {
                    result.Add((entry.Key, value));
                }
            }

            return result;
        }

        public List<double> ConvertHistoricalBatchesToList(double[][] batches)
        {
            var readings = new List<double>();

            // Each inner array represents one batch of telemetry readings.
            foreach (var batch in batches)
            {
                readings.AddRange(batch);
            }

            return readings;
        }
    }
}
