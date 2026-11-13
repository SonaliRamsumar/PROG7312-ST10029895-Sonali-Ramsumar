
namespace SmartX.Client.Models
{
    // Represents one connection between two devices.
    public class NetworkEdge
    {
        public string From { get; set; } = string.Empty;

        public string To { get; set; } = string.Empty;

        public double Weight { get; set; }
    }

    // Represents the current network topology.
    public class NetworkTopologyResponse
    {
        public List<SensorRegistration> Devices { get; set; } = new();

        public List<NetworkEdge> Connections { get; set; } = new();

        public int VertexCount { get; set; }

        public int EdgeCount { get; set; }
    }

    // Represents the minimum spanning tree result.
    public class MinimumSpanningTreeResponse
    {
        public List<NetworkEdge> Connections { get; set; } = new();

        public double TotalWeight { get; set; }

        public int ConnectionCount { get; set; }
    }

    // Represents the result of Dijkstra's shortest-path algorithm.
    public class NetworkPathResult
    {
        public string StartDevice { get; set; } = string.Empty;

        public string DestinationDevice { get; set; } = string.Empty;

        public List<string> Path { get; set; } = new();

        public double? TotalWeight { get; set; }

        public bool Found { get; set; }
    }
}
