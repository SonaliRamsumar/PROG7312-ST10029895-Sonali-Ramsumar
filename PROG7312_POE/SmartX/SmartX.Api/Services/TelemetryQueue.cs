
using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class TelemetryQueue<T> where T : struct
    {
        private readonly Queue<TelemetryPacket<T>> _queue = new();

        public int Count => _queue.Count;

        public void Enqueue(TelemetryPacket<T> packet)
        {
            // Adds new telemetry to the back of the queue.
            _queue.Enqueue(packet);
        }

        public bool TryDequeue(out TelemetryPacket<T>? packet)
        {
            // Removes the oldest packet so it can be processed first.
            return _queue.TryDequeue(out packet);
        }

        public TelemetryPacket<T>[] GetPendingPackets()
        {
            // Returns the waiting packets without removing them.
            return _queue.ToArray();
        }
    }
}
