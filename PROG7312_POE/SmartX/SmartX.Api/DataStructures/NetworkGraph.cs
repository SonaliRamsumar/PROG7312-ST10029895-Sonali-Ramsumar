
using SmartX.Api.Models;

namespace SmartX.Api.DataStructures
{
    // Represents a weighted, undirected network graph.
    public class NetworkGraph
    {
        private readonly Dictionary<string, Dictionary<string, double>>
            _adjacencyList = new(StringComparer.OrdinalIgnoreCase);

        public int VertexCount => _adjacencyList.Count;

        public int EdgeCount =>
            _adjacencyList.Values.Sum(neighbours => neighbours.Count) / 2;

        public void AddVertex(string deviceIdentifier)
        {
            ValidateIdentifier(deviceIdentifier);

            string identifier = deviceIdentifier.Trim();

            if (!_adjacencyList.ContainsKey(identifier))
            {
                _adjacencyList.Add(
                    identifier,
                    new Dictionary<string, double>(
                        StringComparer.OrdinalIgnoreCase));
            }
        }

        public void AddEdge(
            string firstDevice,
            string secondDevice,
            double weight)
        {
            ValidateIdentifier(firstDevice);
            ValidateIdentifier(secondDevice);

            if (firstDevice.Equals(
                secondDevice,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "A device cannot connect to itself.");
            }

            if (double.IsNaN(weight) ||
                double.IsInfinity(weight) ||
                weight < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(weight),
                    "Connection weight must be a non-negative number.");
            }

            string first = firstDevice.Trim();
            string second = secondDevice.Trim();

            AddVertex(first);
            AddVertex(second);

            _adjacencyList[first][second] = weight;
            _adjacencyList[second][first] = weight;
        }

        public List<string> GetVertices()
        {
            return _adjacencyList.Keys
                .OrderBy(device => device)
                .ToList();
        }

        public List<NetworkEdge> GetEdges()
        {
            var edges = new List<NetworkEdge>();

            foreach (var device in _adjacencyList)
            {
                foreach (var neighbour in device.Value)
                {
                    if (StringComparer.OrdinalIgnoreCase.Compare(
                        device.Key, neighbour.Key) < 0)
                    {
                        edges.Add(new NetworkEdge(
                            device.Key,
                            neighbour.Key,
                            neighbour.Value));
                    }
                }
            }

            return edges
                .OrderBy(edge => edge.From)
                .ThenBy(edge => edge.To)
                .ToList();
        }

        public Dictionary<string, double> GetNeighbours(
            string deviceIdentifier)
        {
            ValidateIdentifier(deviceIdentifier);

            if (!_adjacencyList.TryGetValue(
                deviceIdentifier.Trim(), out var neighbours))
            {
                return new Dictionary<string, double>(
                    StringComparer.OrdinalIgnoreCase);
            }

            return new Dictionary<string, double>(
                neighbours,
                StringComparer.OrdinalIgnoreCase);
        }

        // Breadth-First Search visits devices level by level.
        public List<string> BreadthFirstSearch(string startDevice)
        {
            ValidateStartDevice(startDevice);

            var visited = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            var traversalOrder = new List<string>();
            var queue = new Queue<string>();

            string start = startDevice.Trim();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();
                traversalOrder.Add(current);

                foreach (string neighbour in _adjacencyList[current].Keys
                    .OrderBy(device => device))
                {
                    if (visited.Add(neighbour))
                    {
                        queue.Enqueue(neighbour);
                    }
                }
            }

            return traversalOrder;
        }

        // Depth-First Search explores each branch before backtracking.
        public List<string> DepthFirstSearch(string startDevice)
        {
            ValidateStartDevice(startDevice);

            var visited = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            var traversalOrder = new List<string>();

            DepthFirstSearchRecursive(
                startDevice.Trim(),
                visited,
                traversalOrder);

            return traversalOrder;
        }

        private void DepthFirstSearchRecursive(
            string current,
            HashSet<string> visited,
            List<string> traversalOrder)
        {
            visited.Add(current);
            traversalOrder.Add(current);

            foreach (string neighbour in _adjacencyList[current].Keys
                .OrderBy(device => device))
            {
                if (!visited.Contains(neighbour))
                {
                    DepthFirstSearchRecursive(
                        neighbour,
                        visited,
                        traversalOrder);
                }
            }
        }

        // Prim's algorithm builds a minimum spanning forest.
        public List<NetworkEdge> GetMinimumSpanningTree(
            string? startDevice = null)
        {
            if (!string.IsNullOrWhiteSpace(startDevice) &&
                !_adjacencyList.ContainsKey(startDevice.Trim()))
            {
                throw new ArgumentException(
                    "The starting device does not exist in the graph.",
                    nameof(startDevice));
            }

            var minimumTree = new List<NetworkEdge>();

            var visited = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            var startingOrder = _adjacencyList.Keys
                .OrderBy(device => device)
                .ToList();

            if (!string.IsNullOrWhiteSpace(startDevice))
            {
                string selectedStart = startDevice.Trim();

                startingOrder.RemoveAll(device =>
                    device.Equals(
                        selectedStart,
                        StringComparison.OrdinalIgnoreCase));

                startingOrder.Insert(0, selectedStart);
            }

            long insertionOrder = 0;

            foreach (string componentStart in startingOrder)
            {
                if (!visited.Add(componentStart))
                {
                    continue;
                }

                var priorityQueue =
                    new PriorityQueue<NetworkEdge, (double Weight, long Order)>();

                AddAvailableEdges(componentStart);

                while (priorityQueue.Count > 0)
                {
                    NetworkEdge edge = priorityQueue.Dequeue();

                    if (!visited.Add(edge.To))
                    {
                        continue;
                    }

                    minimumTree.Add(edge);
                    AddAvailableEdges(edge.To);
                }

                void AddAvailableEdges(string current)
                {
                    foreach (var neighbour in _adjacencyList[current]
                        .OrderBy(item => item.Key))
                    {
                        if (!visited.Contains(neighbour.Key))
                        {
                            var edge = new NetworkEdge(
                                current,
                                neighbour.Key,
                                neighbour.Value);

                            priorityQueue.Enqueue(
                                edge,
                                (neighbour.Value, insertionOrder++));
                        }
                    }
                }
            }

            return minimumTree;
        }

        // Dijkstra calculates the lowest-cost route between two devices.
        public NetworkPathResult FindShortestPath(
            string startDevice,
            string destinationDevice)
        {
            ValidateStartDevice(startDevice);
            ValidateStartDevice(destinationDevice);

            string start = startDevice.Trim();
            string destination = destinationDevice.Trim();

            var distances = new Dictionary<string, double>(
                StringComparer.OrdinalIgnoreCase);

            var previous = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (string device in _adjacencyList.Keys)
            {
                distances[device] = double.PositiveInfinity;
            }

            distances[start] = 0;

            var priorityQueue = new PriorityQueue<string, double>();
            priorityQueue.Enqueue(start, 0);

            while (priorityQueue.Count > 0)
            {
                priorityQueue.TryDequeue(
                    out string? current,
                    out double currentDistance);

                if (current == null)
                {
                    continue;
                }

                // Ignore outdated queue entries.
                if (currentDistance > distances[current])
                {
                    continue;
                }

                // The shortest route to the destination is now known.
                if (current.Equals(
                    destination,
                    StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                foreach (var neighbour in _adjacencyList[current])
                {
                    double newDistance =
                        currentDistance + neighbour.Value;

                    if (newDistance < distances[neighbour.Key])
                    {
                        distances[neighbour.Key] = newDistance;
                        previous[neighbour.Key] = current;

                        priorityQueue.Enqueue(
                            neighbour.Key,
                            newDistance);
                    }
                }
            }

            // No route exists between the selected devices.
            if (double.IsPositiveInfinity(distances[destination]))
            {
                return new NetworkPathResult
                {
                    StartDevice = start,
                    DestinationDevice = destination,
                    Found = false,
                    TotalWeight = null,
                    Path = new List<string>()
                };
            }

            // Reconstruct the route from destination to start.
            var path = new List<string>();
            string currentDevice = destination;

            path.Add(currentDevice);

            while (!currentDevice.Equals(
                start,
                StringComparison.OrdinalIgnoreCase))
            {
                if (!previous.TryGetValue(
                    currentDevice, out string? previousDevice))
                {
                    return new NetworkPathResult
                    {
                        StartDevice = start,
                        DestinationDevice = destination,
                        Found = false,
                        TotalWeight = null,
                        Path = new List<string>()
                    };
                }

                currentDevice = previousDevice;
                path.Add(currentDevice);
            }

            path.Reverse();

            return new NetworkPathResult
            {
                StartDevice = start,
                DestinationDevice = destination,
                Path = path,
                TotalWeight = distances[destination],
                Found = true
            };
        }

        private void ValidateStartDevice(string startDevice)
        {
            ValidateIdentifier(startDevice);

            if (!_adjacencyList.ContainsKey(startDevice.Trim()))
            {
                throw new ArgumentException(
                    "The selected device does not exist in the graph.",
                    nameof(startDevice));
            }
        }

        private static void ValidateIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentException(
                    "Device identifier cannot be empty.");
            }
        }
    }

    public record NetworkEdge(
        string From,
        string To,
        double Weight);
}
