
using SmartX.Api.DataStructures;
using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class TelemetryHistoryService
    {
        private sealed record HistoricalEntry(
            long Sequence,
            TelemetryPacket<double> Packet);

        private readonly Dictionary<string,
            AvlTree<(DateTime Timestamp, long Sequence), HistoricalEntry>> _deviceHistory =
                new(StringComparer.OrdinalIgnoreCase);

        private readonly object _lock = new();
        private long _nextSequence;

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _deviceHistory.Values.Sum(tree => tree.Count);
                }
            }
        }

        public void AddReading(TelemetryPacket<double> packet)
        {
            if (packet == null ||
                string.IsNullOrWhiteSpace(packet.DeviceIdentifier))
            {
                return;
            }

            lock (_lock)
            {
                string deviceIdentifier = packet.DeviceIdentifier.Trim();

                if (!_deviceHistory.TryGetValue(
                    deviceIdentifier, out var tree))
                {
                    tree = new AvlTree<
                        (DateTime Timestamp, long Sequence),
                        HistoricalEntry>();

                    _deviceHistory.Add(deviceIdentifier, tree);
                }

                long sequence = ++_nextSequence;

                var entry = new HistoricalEntry(sequence, packet);

                tree.Insert(
                    (packet.Timestamp, sequence),
                    entry);
            }
        }

        // Returns all readings in chronological order.
        public List<TelemetryPacket<double>> GetReadingsInChronologicalOrder(
            string? deviceIdentifier = null,
            DateTime? from = null,
            DateTime? to = null)
        {
            if (from.HasValue &&
                to.HasValue &&
                from.Value > to.Value)
            {
                return new List<TelemetryPacket<double>>();
            }

            lock (_lock)
            {
                var minimumKey = (
                    Timestamp: from ?? DateTime.MinValue,
                    Sequence: long.MinValue);

                var maximumKey = (
                    Timestamp: to ?? DateTime.MaxValue,
                    Sequence: long.MaxValue);

                IEnumerable<HistoricalEntry> entries;

                if (!string.IsNullOrWhiteSpace(deviceIdentifier))
                {
                    if (!_deviceHistory.TryGetValue(
                        deviceIdentifier.Trim(), out var deviceTree))
                    {
                        return new List<TelemetryPacket<double>>();
                    }

                    entries = deviceTree.GetRange(
                        minimumKey,
                        maximumKey);
                }
                else
                {
                    entries = _deviceHistory.Values
                        .SelectMany(tree => tree.GetRange(
                            minimumKey,
                            maximumKey));
                }

                return entries
                    .OrderBy(entry => entry.Packet.Timestamp)
                    .ThenBy(entry => entry.Sequence)
                    .Select(entry => entry.Packet)
                    .ToList();
            }
        }

        // Returns readings for one device.
        public List<TelemetryPacket<double>> GetReadingsForDevice(
            string deviceIdentifier)
        {
            if (string.IsNullOrWhiteSpace(deviceIdentifier))
            {
                return new List<TelemetryPacket<double>>();
            }

            return GetReadingsInChronologicalOrder(deviceIdentifier);
        }

        // Returns readings within an inclusive date range.
        public List<TelemetryPacket<double>> GetReadingsInRange(
            DateTime start,
            DateTime end,
            string? deviceIdentifier = null)
        {
            if (start > end)
            {
                return new List<TelemetryPacket<double>>();
            }

            return GetReadingsInChronologicalOrder(
                deviceIdentifier,
                start,
                end);
        }

        // Returns the height of one device's AVL tree.
        public int GetTreeHeight(string deviceIdentifier)
        {
            lock (_lock)
            {
                if (string.IsNullOrWhiteSpace(deviceIdentifier) ||
                    !_deviceHistory.TryGetValue(
                        deviceIdentifier.Trim(), out var tree))
                {
                    return 0;
                }

                return tree.Height;
            }
        }

        // Converts historical batches into a standard list.
        public List<double> ConvertHistoricalBatchesToList(
            double[][] batches)
        {
            var readings = new List<double>();

            foreach (var batch in batches)
            {
                readings.AddRange(batch);
            }

            return readings;
        }
    }
}
