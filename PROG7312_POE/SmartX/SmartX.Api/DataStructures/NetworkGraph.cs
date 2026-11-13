
namespace SmartX.Api.DataStructures
{
    // Represents a weighted, undirected network graph.
    // Each vertex represents a device and each edge represents a connection.
    public class NetworkGraph
    {
        private readonly Dictionary<string, Dictionary<string, double>>
            _adjacencyList = new(StringComparer.OrdinalIgnoreCase);

        // Stores the number of registered vertices.
        public int VertexCount => _adjacencyList.Count;

        // Counts each undirected connection only once.
        public int EdgeCount =>
            _adjacencyList.Values.Sum(neighbours => neighbours.Count) / 2;

        // Adds a device to the graph.
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

        // Adds an undirected connection between two devices.
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

            // Make sure both devices exist before connecting them.
            AddVertex(first);
            AddVertex(second);

            // Store the connection in both directions.
            _adjacencyList[first][second] = weight;
            _adjacencyList[second][first] = weight;
        }

        // Returns all device identifiers in the graph.
        public List<string> GetVertices()
        {
            return _adjacencyList.Keys
                .OrderBy(device => device)
                .ToList();
        }

        // Returns the direct connections for a selected device.
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

        // Breadth-First Search visits neighbouring devices level by level.
        public List<string> BreadthFirstSearch(string startDevice)
        {
            ValidateStartDevice(startDevice);

            var visited = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            var traversalOrder = new List<string>();
            var queue = new Queue<string>();

            queue.Enqueue(startDevice.Trim());
            visited.Add(startDevice.Trim());

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

        // Depth-First Search explores one branch before backtracking.
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

        // Prim's algorithm creates a minimum spanning forest.
        // For a connected graph, the result is a minimum spanning tree.
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

            // If a starting device is supplied, visit its component first.
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
                // Skip devices already included in another component.
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

                    // Ignore edges leading to an already visited device.
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

        private void ValidateStartDevice(string startDevice)
        {
            ValidateIdentifier(startDevice);

            if (!_adjacencyList.ContainsKey(startDevice.Trim()))
            {
                throw new ArgumentException(
                    "The starting device does not exist in the graph.",
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

    // Represents one weighted connection between two devices.
    public record NetworkEdge(
        string From,
        string To,
        double Weight);
}
