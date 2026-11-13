
using SmartX.Api.DataStructures;
using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class NetworkTopologyService
    {
        private readonly SensorService _sensorService;
        private readonly NetworkGraph _networkGraph;
        private readonly object _lock = new();

        public NetworkTopologyService(
            SensorService sensorService,
            NetworkGraph networkGraph)
        {
            _sensorService = sensorService;
            _networkGraph = networkGraph;
        }

        // Adds every registered sensor as a graph vertex.
        private void SynchroniseSensors()
        {
            foreach (var sensor in _sensorService.GetAll())
            {
                _networkGraph.AddVertex(sensor.DeviceIdentifier);
            }
        }

        // Returns registered devices and their network connections.
        public NetworkTopologyResponse GetTopology()
        {
            lock (_lock)
            {
                SynchroniseSensors();

                var sensors = _sensorService.GetAll();

                return new NetworkTopologyResponse
                {
                    Devices = sensors,
                    Connections = _networkGraph.GetEdges(),
                    VertexCount = _networkGraph.VertexCount,
                    EdgeCount = _networkGraph.EdgeCount
                };
            }
        }

        // Creates a connection between two registered sensors.
        public void AddConnection(
            string firstDevice,
            string secondDevice,
            double weight)
        {
            lock (_lock)
            {
                SynchroniseSensors();

                if (!_sensorService.DeviceExists(firstDevice))
                {
                    throw new ArgumentException(
                        $"Device '{firstDevice}' is not registered.");
                }

                if (!_sensorService.DeviceExists(secondDevice))
                {
                    throw new ArgumentException(
                        $"Device '{secondDevice}' is not registered.");
                }

                _networkGraph.AddEdge(
                    firstDevice,
                    secondDevice,
                    weight);
            }
        }

        // Runs Breadth-First Search from a registered device.
        public List<string> RunBreadthFirstSearch(string startDevice)
        {
            lock (_lock)
            {
                SynchroniseSensors();

                return _networkGraph.BreadthFirstSearch(startDevice);
            }
        }

        // Runs Depth-First Search from a registered device.
        public List<string> RunDepthFirstSearch(string startDevice)
        {
            lock (_lock)
            {
                SynchroniseSensors();

                return _networkGraph.DepthFirstSearch(startDevice);
            }
        }

        // Calculates the minimum spanning tree or forest.
        public List<NetworkEdge> GetMinimumSpanningTree(
            string? startDevice = null)
        {
            lock (_lock)
            {
                SynchroniseSensors();

                return _networkGraph.GetMinimumSpanningTree(startDevice);
            }
        }
    }
}
