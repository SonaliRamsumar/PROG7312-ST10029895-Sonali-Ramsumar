
using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class TelemetryQueue<T> where T : struct
    {
        // Stores telemetry packets in the order they arrive.
        private readonly Queue<TelemetryPacket<T>> _queue = new();

        // Prevents the API and background service from accessing
        // the queue at exactly the same time.
        private readonly object _lock = new();

        // Returns the number of packets currently waiting.
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }

        public void Enqueue(TelemetryPacket<T> packet)
        {
            ArgumentNullException.ThrowIfNull(packet);

            lock (_lock)
            {
                // Adds the new packet to the back of the FIFO queue.
                _queue.Enqueue(packet);
            }
        }

        public bool TryDequeue(out TelemetryPacket<T>? packet)
        {
            lock (_lock)
            {
                // Removes the oldest packet from the queue.
                return _queue.TryDequeue(out packet);
            }
        }

        public TelemetryPacket<T>[] GetPendingPackets()
        {
            lock (_lock)
            {
                // Returns a copy without removing the waiting packets.
                return _queue.ToArray();
            }
        }
    }
}
