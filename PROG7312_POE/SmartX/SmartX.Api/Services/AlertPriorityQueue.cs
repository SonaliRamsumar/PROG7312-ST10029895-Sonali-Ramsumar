
using SmartX.Api.Models;
using System.Globalization;

namespace SmartX.Api.Services
{
    public class AlertPriorityQueue
    {
        // Stores alerts according to their priority.
        private readonly PriorityQueue<TelemetryAlert, int> _alerts = new();

        // Tracks alerts that are already waiting in the queue.
        private readonly HashSet<string> _pendingAlertKeys =
            new(StringComparer.OrdinalIgnoreCase);

        // Remembers alerts that have already been processed.
        private readonly HashSet<string> _processedAlertKeys =
            new(StringComparer.OrdinalIgnoreCase);

        // Synchronises access when the API and background service
        // work with the priority queue simultaneously.
        private readonly object _lock = new();

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _alerts.Count;
                }
            }
        }

        public int ProcessedAlertCount
        {
            get
            {
                lock (_lock)
                {
                    return _processedAlertKeys.Count;
                }
            }
        }

        public bool EnqueueAlert(TelemetryAlert alert)
        {
            ArgumentNullException.ThrowIfNull(alert);

            lock (_lock)
            {
                string eventKey = CreateEventKey(alert);

                // Do not enqueue an event that was already processed.
                if (_processedAlertKeys.Contains(eventKey))
                {
                    return false;
                }

                string pendingKey = CreatePendingKey(alert);

                // Prevent identical alerts from waiting in the queue twice.
                if (!_pendingAlertKeys.Add(pendingKey))
                {
                    return false;
                }

                int priority = GetPriority(alert.Severity);

                // Add the alert using its calculated priority.
                _alerts.Enqueue(alert, priority);

                return true;
            }
        }

        public bool TryDequeueAlert(out TelemetryAlert? alert)
        {
            lock (_lock)
            {
                while (_alerts.TryDequeue(out var next, out _))
                {
                    if (next == null)
                    {
                        continue;
                    }

                    string eventKey = CreateEventKey(next);
                    string pendingKey = CreatePendingKey(next);

                    // Remove the alert from the pending collection.
                    _pendingAlertKeys.Remove(pendingKey);

                    // Prevent the same event from being processed twice.
                    if (!_processedAlertKeys.Add(eventKey))
                    {
                        continue;
                    }

                    alert = next;
                    return true;
                }

                alert = null;
                return false;
            }
        }

        public TelemetryAlert? PeekNextAlert()
        {
            lock (_lock)
            {
                return _alerts.TryPeek(
                    out TelemetryAlert? alert,
                    out _)
                    ? alert
                    : null;
            }
        }

        public TelemetryAlert[] GetPendingAlertsInPriorityOrder()
        {
            lock (_lock)
            {
                // Create a temporary queue so the original is not changed.
                var copy = new PriorityQueue<TelemetryAlert, int>();

                foreach (var item in _alerts.UnorderedItems)
                {
                    copy.Enqueue(item.Element, item.Priority);
                }

                var pendingAlerts = new List<TelemetryAlert>();

                // Remove items from the temporary queue in priority order.
                while (copy.TryDequeue(out var alert, out _))
                {
                    if (alert != null)
                    {
                        pendingAlerts.Add(alert);
                    }
                }

                return pendingAlerts.ToArray();
            }
        }

        private static string CreatePendingKey(TelemetryAlert alert)
        {
            string device = alert.DeviceIdentifier.Trim().ToUpperInvariant();
            string severity = alert.Severity.Trim().ToUpperInvariant();

            string value = alert.Value.ToString(
                "R",
                CultureInfo.InvariantCulture);

            return $"{device}|{severity}|{value}";
        }

        private static string CreateEventKey(TelemetryAlert alert)
        {
            // The timestamp distinguishes separate alert occurrences.
            return $"{CreatePendingKey(alert)}|{alert.Timestamp.ToUniversalTime().Ticks}";
        }

        private static int GetPriority(string severity)
        {
            return (severity ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "critical" => 1,
                "warning" => 2,
                "normal" => 3,
                _ => 4
            };
        }
    }
}
