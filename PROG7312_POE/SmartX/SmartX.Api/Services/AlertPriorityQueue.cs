using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class AlertPriorityQueue
    {
        // Stores alerts according to their priority.
        private readonly PriorityQueue<TelemetryAlert, int> _alerts = new();

        public int Count => _alerts.Count;

        public void EnqueueAlert(TelemetryAlert alert)
        {
            int priority = GetPriority(alert.Severity);

            // A smaller number means the alert is more urgent.
            _alerts.Enqueue(alert, priority);
        }

        public bool TryDequeueAlert(out TelemetryAlert? alert)
        {
            // Removes the most urgent alert waiting in the queue.
            return _alerts.TryDequeue(out alert, out _);
        }

        public TelemetryAlert? PeekNextAlert()
        {
            // Checks the next alert without removing it.
            return _alerts.TryPeek(out TelemetryAlert? alert, out _)
                ? alert
                : null;
        }

        public TelemetryAlert[] GetPendingAlertsInPriorityOrder()
        {
            // Copy the queue so we can sort the results without
            // removing alerts from the original queue.
            var copy = new PriorityQueue<TelemetryAlert, int>();

            foreach (var item in _alerts.UnorderedItems)
            {
                copy.Enqueue(item.Element, item.Priority);
            }

            var pendingAlerts = new List<TelemetryAlert>();

            while (copy.TryDequeue(out TelemetryAlert? alert, out _))
            {
                if (alert != null)
                {
                    pendingAlerts.Add(alert);
                }
            }

            return pendingAlerts.ToArray();
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