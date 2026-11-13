
namespace SmartX.Client.Models
{
    // Represents one connection between two devices.
    public class NetworkEdge
    {
        public string From { get; set; } = string.Empty;

        public string To { get; set; } = string.Empty;

        public double Weight { get; set; }
    }

    // Represents the topology information returned by the API.
    public class NetworkTopologyResponse
    {
        public List<SensorRegistration> Devices { get; set; } = new();

        public List<NetworkEdge> Connections { get; set; } = new();

        public int VertexCount { get; set; }

        public int EdgeCount { get; set; }
    }

    // Represents the result returned by the minimum spanning tree endpoint.
    public class MinimumSpanningTreeResponse
    {
        public List<NetworkEdge> Connections { get; set; } = new();

        public double TotalWeight { get; set; }

        public int ConnectionCount { get; set; }
    }
}
