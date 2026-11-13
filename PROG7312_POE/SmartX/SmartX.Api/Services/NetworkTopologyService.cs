
using System.Text.Json;
using SmartX.Api.DataStructures;
using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class NetworkTopologyService
    {
        private readonly SensorService _sensorService;
        private readonly NetworkGraph _networkGraph;
        private readonly object _lock = new();

        private readonly string _storageFilePath;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public NetworkTopologyService(
            SensorService sensorService,
            NetworkGraph networkGraph,
            IWebHostEnvironment environment)
        {
            _sensorService = sensorService;
            _networkGraph = networkGraph;

            _storageFilePath = Path.Combine(
                environment.ContentRootPath,
                "App_Data",
                "network-connections.json");

            lock (_lock)
            {
                SynchroniseSensors();
                LoadConnections();
            }
        }

        // Adds registered sensors to the graph as vertices.
        private void SynchroniseSensors()
        {
            foreach (var sensor in _sensorService.GetAll())
            {
                _networkGraph.AddVertex(sensor.DeviceIdentifier);
            }
        }

        // Returns the current network topology.
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

        // Creates a connection and saves it to JSON.
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

                // AddEdge validates the identifiers and weight.
                _networkGraph.AddEdge(
                    firstDevice,
                    secondDevice,
                    weight);

                // Save the updated graph after creating the connection.
                SaveConnections();
            }
        }

        // Runs Breadth-First Search.
        public List<string> RunBreadthFirstSearch(string startDevice)
        {
            lock (_lock)
            {
                SynchroniseSensors();

                return _networkGraph.BreadthFirstSearch(startDevice);
            }
        }

        // Runs Depth-First Search.
        public List<string> RunDepthFirstSearch(string startDevice)
        {
            lock (_lock)
            {
                SynchroniseSensors();

                return _networkGraph.DepthFirstSearch(startDevice);
            }
        }

        // Calculates Prim's minimum spanning tree or forest.
        public List<NetworkEdge> GetMinimumSpanningTree(
            string? startDevice = null)
        {
            lock (_lock)
            {
                SynchroniseSensors();

                return _networkGraph.GetMinimumSpanningTree(startDevice);
            }
        }

        // Finds the lowest-cost route between two registered devices.
        public NetworkPathResult FindShortestPath(
            string startDevice,
            string destinationDevice)
        {
            lock (_lock)
            {
                SynchroniseSensors();

                if (!_sensorService.DeviceExists(startDevice))
                {
                    throw new ArgumentException(
                        $"Device '{startDevice}' is not registered.");
                }

                if (!_sensorService.DeviceExists(destinationDevice))
                {
                    throw new ArgumentException(
                        $"Device '{destinationDevice}' is not registered.");
                }

                return _networkGraph.FindShortestPath(
                    startDevice,
                    destinationDevice);
            }
        }

        // Loads previously saved connections when the service starts.
        private void LoadConnections()
        {
            if (!File.Exists(_storageFilePath))
            {
                return;
            }

            try
            {
                string json = File.ReadAllText(_storageFilePath);

                var savedConnections =
                    JsonSerializer.Deserialize<List<NetworkEdge>>(
                        json,
                        _jsonOptions);

                if (savedConnections == null)
                {
                    return;
                }

                foreach (var connection in savedConnections)
                {
                    if (connection == null ||
                        string.IsNullOrWhiteSpace(connection.From) ||
                        string.IsNullOrWhiteSpace(connection.To))
                    {
                        throw new InvalidDataException(
                            "The saved network file contains an invalid connection.");
                    }

                    // Do not restore connections for devices that no longer exist.
                    if (!_sensorService.DeviceExists(connection.From) ||
                        !_sensorService.DeviceExists(connection.To))
                    {
                        continue;
                    }

                    _networkGraph.AddEdge(
                        connection.From,
                        connection.To,
                        connection.Weight);
                }
            }
            catch (JsonException exception)
            {
                throw new InvalidOperationException(
                    "The saved network connections could not be read. " +
                    "Check App_Data/network-connections.json.",
                    exception);
            }
        }

        // Saves all network connections to the JSON file.
        private void SaveConnections()
        {
            string? temporaryFile = null;

            try
            {
                string directory =
                    Path.GetDirectoryName(_storageFilePath)!;

                Directory.CreateDirectory(directory);

                temporaryFile = _storageFilePath + ".tmp";

                var connectionsToSave = _networkGraph.GetEdges();

                string json = JsonSerializer.Serialize(
                    connectionsToSave,
                    _jsonOptions);

                File.WriteAllText(temporaryFile, json);

                // Replace the previous file with the updated version.
                File.Move(
                    temporaryFile,
                    _storageFilePath,
                    overwrite: true);
            }
            finally
            {
                // Remove an unfinished temporary file if saving failed.
                if (temporaryFile != null &&
                    File.Exists(temporaryFile))
                {
                    File.Delete(temporaryFile);
                }
            }
        }
    }
}
