
using SmartX.Api.DataStructures;
using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class TelemetryHistoryService
    {
        // Keeps the original insertion order when timestamps are identical.
        private sealed record HistoricalEntry(
            long Sequence,
            TelemetryPacket<double> Packet);

        // Each device has its own AVL tree, ordered by timestamp and sequence.
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

                // The sequence makes the key unique when timestamps match.
                tree.Insert(
                    (packet.Timestamp, sequence),
                    entry);
            }
        }

        public List<TelemetryPacket<double>> GetReadingsInChronologicalOrder(
            string? deviceIdentifier = null,
            DateTime? from = null,
            DateTime? to = null)
        {
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
                    // Search only the selected device's AVL tree.
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
                    // Search each device tree and combine the results.
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

        // Returns the height of one device's AVL tree.
        // This can be used to demonstrate that the tree remains balanced.
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

        // Retained for the existing jagged-array conversion requirement.
        public List<double> ConvertHistoricalBatchesToList(double[][] batches)
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
