
using SmartX.Api.Models;
using System.Globalization;

namespace SmartX.Api.Services
{
    public class AlertPriorityQueue
    {
        private readonly PriorityQueue<TelemetryAlert, int> _alerts = new();

        // Tracks alert types that are already waiting in the queue.
        private readonly HashSet<string> _pendingAlertKeys =
            new(StringComparer.OrdinalIgnoreCase);

        // Remembers processed alert events so the same event cannot be processed twice.
        private readonly HashSet<string> _processedAlertKeys =
            new(StringComparer.OrdinalIgnoreCase);

        public int Count => _alerts.Count;

        public int ProcessedAlertCount => _processedAlertKeys.Count;

        public bool EnqueueAlert(TelemetryAlert alert)
        {
            string eventKey = CreateEventKey(alert);

            // Do not enqueue an event that has already been processed.
            if (_processedAlertKeys.Contains(eventKey))
            {
                return false;
            }

            string pendingKey = CreatePendingKey(alert);

            // Prevent the same device alert from being queued repeatedly
            // while an identical alert is already waiting.
            if (!_pendingAlertKeys.Add(pendingKey))
            {
                return false;
            }

            int priority = GetPriority(alert.Severity);
            _alerts.Enqueue(alert, priority);

            return true;
        }

        public bool TryDequeueAlert(out TelemetryAlert? alert)
        {
            while (_alerts.TryDequeue(out var next, out _))
            {
                if (next == null)
                {
                    continue;
                }

                string eventKey = CreateEventKey(next);
                string pendingKey = CreatePendingKey(next);

                _pendingAlertKeys.Remove(pendingKey);

                // Record this event as processed before returning it.
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

        public TelemetryAlert? PeekNextAlert()
        {
            return _alerts.TryPeek(out TelemetryAlert? alert, out _)
                ? alert
                : null;
        }

        public TelemetryAlert[] GetPendingAlertsInPriorityOrder()
        {
            // Sort a copy so the original queue remains unchanged.
            var copy = new PriorityQueue<TelemetryAlert, int>();

            foreach (var item in _alerts.UnorderedItems)
            {
                copy.Enqueue(item.Element, item.Priority);
            }

            var pendingAlerts = new List<TelemetryAlert>();

            while (copy.TryDequeue(out var alert, out _))
            {
                if (alert != null)
                {
                    pendingAlerts.Add(alert);
                }
            }

            return pendingAlerts.ToArray();
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
            // The timestamp distinguishes separate occurrences of an alert.
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
