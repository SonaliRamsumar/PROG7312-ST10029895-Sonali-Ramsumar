
using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class TelemetryHistoryService
    {
        // Each device has its own timestamp-sorted collection.
        // Multiple readings can share the same timestamp.
        private readonly Dictionary<string, SortedDictionary<DateTime, List<TelemetryPacket<double>>>> _deviceHistory =
            new(StringComparer.OrdinalIgnoreCase);

        // Protects the history while API requests read and write it.
        private readonly object _lock = new();

        // Counts all readings across every registered device.
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _deviceHistory.Values
                        .SelectMany(timeline => timeline.Values)
                        .Sum(readings => readings.Count);
                }
            }
        }

        // Stores a complete packet so its device identifier is never lost.
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

                // Create a separate timeline for a device's first reading.
                if (!_deviceHistory.TryGetValue(
                    deviceIdentifier,
                    out var timeline))
                {
                    timeline = new SortedDictionary<DateTime, List<TelemetryPacket<double>>>();
                    _deviceHistory.Add(deviceIdentifier, timeline);
                }

                // Create a list when this timestamp is first encountered.
                if (!timeline.TryGetValue(packet.Timestamp, out var readings))
                {
                    readings = new List<TelemetryPacket<double>>();
                    timeline.Add(packet.Timestamp, readings);
                }

                // Store a copy so later changes to the original packet
                // cannot accidentally change the historical record.
                readings.Add(new TelemetryPacket<double>
                {
                    DeviceIdentifier = deviceIdentifier,
                    Timestamp = packet.Timestamp,
                    Value = packet.Value
                });
            }
        }

        // Returns all devices' readings in chronological order.
        public List<TelemetryPacket<double>> GetReadingsInChronologicalOrder()
        {
            lock (_lock)
            {
                return _deviceHistory.Values
                    .SelectMany(timeline => timeline.Values)
                    .SelectMany(readings => readings)
                    .OrderBy(packet => packet.Timestamp)
                    .ThenBy(packet => packet.DeviceIdentifier)
                    .ToList();
            }
        }

        // Returns the historical timeline for one device.
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

                return timeline.Values
                    .SelectMany(readings => readings)
                    .ToList();
            }
        }

        // Retrieves readings between two dates, optionally for one device.
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

            IEnumerable<TelemetryPacket<double>> readings =
                string.IsNullOrWhiteSpace(deviceIdentifier)
                    ? GetReadingsInChronologicalOrder()
                    : GetReadingsForDevice(deviceIdentifier);

            return readings
                .Where(packet =>
                    packet.Timestamp >= start &&
                    packet.Timestamp <= end)
                .OrderBy(packet => packet.Timestamp)
                .ToList();
        }

        // Combines multiple batches into one list for the existing
        // Part 1 historical batch-processing requirement.
        public List<double> ConvertHistoricalBatchesToList(double[][] batches)
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
