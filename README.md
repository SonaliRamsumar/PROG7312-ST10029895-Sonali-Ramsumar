# Smart-X IoT Management System

Smart-X is a .NET 10 IoT Management System developed to register IoT sensors, monitor telemetry readings, process device commands, manage sensor-related files and monitor network connections. The system uses an ASP.NET Core Web API backend and a Blazor WebAssembly frontend.

## Technologies Used

- .NET 10
- C#
- ASP.NET Core Web API
- Blazor WebAssembly
- Firebase Authentication
- JSON data storage
- GitHub
- Visual Studio 2026
- Postman

## Main Features

### Authentication
- Register new users and sign in using Firebase Authentication.
- Provide authenticated access to the main application.
- Send Firebase authentication tokens with API requests.

### Sensor Data Ingestion and Telemetry
- Register IoT sensors and view registered devices.
- Upload sensor configuration files and other sensor-related files.
- Automatically register sensors by parsing supported `.txt` files.
- Generate simulated temperature readings through the API.
- Display live telemetry readings and temperature charts.
- Show Normal, Warning and Critical alert states.
- View historical telemetry readings and retrieve readings using date ranges.
- Save registered sensors and historical temperature readings using JSON files.
- Restore saved sensor and telemetry information when the API starts.

### Real-Time Command Stream and History
- View registered devices and their current relay states.
- Execute commands to switch device relays On or Off.
- View command history and undo the most recent command.
- Search for devices and filter them by category and relay state.
- Process telemetry through queues and manage alerts using a priority queue.
- Use a stack to manage command history and undo operations.
- Record user searches and commands to support predictive action suggestions.

### Network Topology and Mesh Routing
- Display registered IoT devices as nodes in a network graph.
- Create connections between devices and assign connection weights.
- Save network connections using JSON storage.
- Use Breadth-First Search (BFS) and Depth-First Search (DFS) to explore the network.
- Use Prim's Minimum Spanning Tree algorithm to calculate the minimum-cost network connections.
- Use Dijkstra's algorithm to find the shortest route between two devices.
- Display network connections, traversal results and routing information on a dedicated dashboard.

## Smart-X Architecture

The application is organised into three main areas:

1. **Sensor Data Ingestion and Telemetry** - supports sensor registration, file uploads, live readings, historical telemetry and alerts.
2. **Real-Time Command Stream and History** - supports device commands, command history, undo operations and predictive suggestions.
3. **Network Topology and Mesh Routing** - supports network connections, graph traversal and routing calculations.

## Technical Concepts Demonstrated

### Generic Telemetry Processing

The `TelemetryPacket<T>` class uses generics to handle different types of sensor readings, including numeric temperature values and Boolean switch states.

### Operator Overloading

The `TelemetryReading` class overloads the `+`, `-`, `>` and `<` operators. This allows telemetry readings to be combined and compared using standard C# operators.

### Arrays and Recursion

Arrays are used to store and process data. Recursive methods are used for location validation.

### Jagged Arrays and Historical Telemetry

Jagged arrays are used to represent historical telemetry readings grouped into batches. These batches can be converted into collections for further processing.

### Collections

The project demonstrates the use of different C# collections:

- `List<T>` for working with groups of telemetry readings.
- `Queue<T>` for processing telemetry in first-in, first-out order.
- `PriorityQueue<TElement, TPriority>` for prioritising alerts.
- `Stack<T>` for managing command history and undo operations.
- `Dictionary<TKey, TValue>` for storing device and command state information.
- `HashSet<T>` for tracking unique entries and preventing duplicates where required.

### AVL Trees

The `AvlTree<TKey, TValue>` class is used to store historical telemetry readings.

Each registered device has its own AVL tree, with readings organised using their timestamps. The tree automatically balances itself when new readings are inserted.

This allows the application to retrieve historical readings in chronological order and search for readings within a selected date range without having to go through an entire unsorted list.

### Graphs and Network Traversal

The `NetworkGraph` class represents the IoT mesh network using a graph structure.

- Vertices represent registered IoT devices.
- Edges represent connections between devices.
- Weights represent the cost of connections.

The graph uses different algorithms to explore and calculate routes through the network.

**Breadth-First Search (BFS)**

BFS explores connected devices level by level. It can be used to identify which devices are reachable from a selected starting device.

**Depth-First Search (DFS)**

DFS explores one branch of the network before moving back to explore other connections.

**Prim's Minimum Spanning Tree**

Prim's algorithm is used to find a minimum spanning tree by selecting connections with the lowest combined weight while connecting the reachable devices.

**Dijkstra's Shortest-Path Algorithm**

Dijkstra's algorithm is used to calculate the lowest-cost route between a selected starting device and destination device.

### Background Telemetry Processing

The `TelemetryProcessingBackgroundService` processes telemetry readings and alerts in the background.

It checks for pending work every second, processes alerts according to their priority and handles telemetry readings in FIFO order. A limit of 25 readings is used for each processing cycle.

### Predictive Action Engine

The predictive action engine uses recorded search and command activity to generate suggestions. It uses previous user activity to identify actions that may be useful to the user instead of generating completely random suggestions.

### IoT Alerts

Temperature readings are evaluated against configured thresholds to determine whether a reading should be classified as Normal, Warning or Critical.

### JSON Persistence

JSON files are used to save certain application data so that it is not lost whenever the API restarts.

The application uses the following files:

- `App_Data/sensors.json` for registered sensors.
- `App_Data/network-connections.json` for network connections.
- `App_Data/telemetry-history.json` for historical temperature readings.

The API loads the saved information when it starts and updates the files when new information is added.

### File Uploads

The application supports uploading sensor-related files, including text files, logs, configuration files and deployment images.

Supported text files can contain sensor details that the API parses and uses to register devices automatically. Other supported file types can be uploaded and saved by the backend.

## Project Structure

- `SmartX.Api` - ASP.NET Core Web API backend. Handles sensor registration, telemetry, alerts, file uploads, device commands, network connections and supporting services.
- `SmartX.Client` - Blazor WebAssembly frontend. Provides the user interface, navigation, authentication pages, dashboard, sensor management, command stream and network topology pages.

## How to Run the Project

1. Open the Smart-X solution in Visual Studio 2026.
2. Ensure that the .NET 10 SDK is installed.
3. Configure the `SmartX.Api` and `SmartX.Client` projects as startup projects.
4. Configure the Firebase project and enable Email/Password authentication in the Firebase Console.
5. Ensure that the frontend Firebase configuration matches the Firebase project used by the API.
6. Run both projects.
7. Open the Blazor WebAssembly frontend in your browser.
8. Register an account or sign in to access the application.
9. Use the navigation menu to access the Dashboard, Sensors, Files, Command Stream and Network Topology pages.

**Important:** The API must be running for sensor registration, telemetry processing, file uploads, device commands and network operations to work. Registered sensors, network connections and historical temperature readings are saved using JSON files and can be restored when the API restarts. Other temporary information, such as pending queue entries, is not necessarily saved.

## API Information

The backend API runs locally over HTTPS during development.

Example API base address:

`https://localhost:7285/`

Example sensor endpoint:

`GET /api/Sensors`

Example file upload endpoint:

`POST /api/Files/upload`

Example telemetry endpoint:

`POST /api/Telemetry/temperature`

Example historical telemetry endpoint:

`GET /api/Telemetry/history/readings`

Example command endpoint:

`POST /api/Commands/execute`

Authenticated endpoints require a valid Firebase ID token.

OpenAPI documentation is available at:

`/openapi/v1.json`

## Example Sensor Data

Example sensor registration:

- **Sensor Name:** Greenhouse Sensor
- **Device Identifier:** TEMP-001
- **Deployment Location:** Greenhouse 1
- **Category:** Temperature

Example temperature readings:

- `25°C` - Normal
- `38°C` - Warning
- `45°C` - Critical

These examples illustrate the configured alert thresholds. Actual classifications depend on the thresholds implemented in the API.

## Part 1 Demonstration

https://youtu.be/3DCBFUvEJj4

## Author

Sonali Ramsumar
