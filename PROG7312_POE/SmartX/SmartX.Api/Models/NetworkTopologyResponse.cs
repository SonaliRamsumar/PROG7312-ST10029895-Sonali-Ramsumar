
using SmartX.Api.DataStructures;

namespace SmartX.Api.Models
{
    public class NetworkTopologyResponse
    {
        public List<SensorRegistration> Devices { get; set; } = new();

        public List<NetworkEdge> Connections { get; set; } = new();

        public int VertexCount { get; set; }

        public int EdgeCount { get; set; }
    }
}
