
using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class TelemetryHistoryService
    {
        // Each device has its own sorted historical timeline.
        // The timestamp and sequence number together form a unique key.
        private readonly Dictionary<string,
            SortedDictionary<(DateTime Timestamp, long Sequence),
            TelemetryPacket<double>>> _deviceHistory =
                new(StringComparer.OrdinalIgnoreCase);

        // Protects historical data from simultaneous access.
        private readonly object _lock = new();

        // Generates a unique sequence number for every reading.
        private long _sequenceNumber;

        // Returns the total number of historical readings.
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _deviceHistory.Values.Sum(
                        timeline => timeline.Count);
                }
            }
        }

        // Adds a complete telemetry packet to the correct device timeline.
        public void AddReading(TelemetryPacket<double> packet)
        {
            ArgumentNullException.ThrowIfNull(packet);

            if (string.IsNullOrWhiteSpace(packet.DeviceIdentifier))
            {
                throw new ArgumentException(
                    "A device identifier is required.",
                    nameof(packet));
            }

            lock (_lock)
            {
                string deviceIdentifier = packet.DeviceIdentifier.Trim();

                if (!_deviceHistory.TryGetValue(
                    deviceIdentifier,
                    out var timeline))
                {
                    timeline = new SortedDictionary<
                        (DateTime Timestamp, long Sequence),
                        TelemetryPacket<double>>();

                    _deviceHistory.Add(deviceIdentifier, timeline);
                }

                // The sequence number prevents identical timestamps
                // from overwriting one another.
                long sequence = ++_sequenceNumber;

                var historicalPacket = new TelemetryPacket<double>
                {
                    DeviceIdentifier = deviceIdentifier,
                    Timestamp = packet.Timestamp,
                    Value = packet.Value
                };

                timeline.Add(
                    (packet.Timestamp, sequence),
                    historicalPacket);
            }
        }

        // Retrieves all devices' readings in chronological order.
        public List<TelemetryPacket<double>>
            GetReadingsInChronologicalOrder()
        {
            lock (_lock)
            {
                return _deviceHistory.Values
                    .SelectMany(timeline => timeline.Values)
                    .OrderBy(packet => packet.Timestamp)
                    .ToList();
            }
        }

        // Retrieves historical readings belonging to one device.
        public List<TelemetryPacket<double>> GetReadingsForDevice(
            string deviceIdentifier)
        {
            if (string.IsNullOrWhiteSpace(deviceIdentifier))
            {
                return new List<TelemetryPacket<double>>();
            }

            lock (_lock)
            {
                if (!_deviceHistory.TryGetValue(
                    deviceIdentifier.Trim(),
                    out var timeline))
                {
                    return new List<TelemetryPacket<double>>();
                }

                return timeline.Values.ToList();
            }
        }

        // Retrieves readings within a specified date range.
        // A device identifier can optionally be supplied.
        public List<TelemetryPacket<double>> GetReadingsInRange(
            DateTime start,
            DateTime end,
            string? deviceIdentifier = null)
        {
            if (start > end)
            {
                throw new ArgumentException(
                    "The start date must be before or equal to the end date.");
            }

            lock (_lock)
            {
                var results = new List<TelemetryPacket<double>>();

                if (!string.IsNullOrWhiteSpace(deviceIdentifier))
                {
                    // Search only the selected device's timeline.
                    if (_deviceHistory.TryGetValue(
                        deviceIdentifier.Trim(),
                        out var timeline))
                    {
                        results.AddRange(
                            timeline
                                .Where(entry =>
                                    entry.Key.Timestamp >= start &&
                                    entry.Key.Timestamp <= end)
                                .Select(entry => entry.Value));
                    }
                }
                else
                {
                    // Search every device's timeline.
                    foreach (var timeline in _deviceHistory.Values)
                    {
                        results.AddRange(
                            timeline
                                .Where(entry =>
                                    entry.Key.Timestamp >= start &&
                                    entry.Key.Timestamp <= end)
                                .Select(entry => entry.Value));
                    }
                }

                return results
                    .OrderBy(packet => packet.Timestamp)
                    .ToList();
            }
        }

        // Preserves the original Part 1 batch conversion functionality.
        public List<double> ConvertHistoricalBatchesToList(
            double[][] batches)
        {
            ArgumentNullException.ThrowIfNull(batches);

            var readings = new List<double>();

            foreach (var batch in batches)
            {
                if (batch != null)
                {
                    readings.AddRange(batch);
                }
            }

            return readings;
        }
    }
}
