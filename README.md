# Smart-X IoT Management System

Smart-X is a .NET 10 IoT Management System developed to register IoT sensors, monitor telemetry readings, process device commands, manage sensor-related files and identify unusual sensor conditions. The system uses an ASP.NET Core Web API backend and a Blazor WebAssembly frontend.

## Technologies Used

- .NET 10
- C#
- ASP.NET Core Web API
- Blazor WebAssembly
- Firebase Authentication
- GitHub
- Visual Studio 2026

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
- View telemetry history and process readings using generic classes.

### Real-Time Command Stream and History
- View registered devices and their current relay states.
- Execute commands to switch device relays On or Off.
- View command history and undo the most recent command.
- Search for devices and filter them by category and relay state.
- Process telemetry through queues and manage alerts using a priority queue.
- Use a stack to manage command history and undo operations.
- Record user searches and commands to support predictive action suggestions.

### Network Topology and Mesh Routing
- Display the third architecture area in the application navigation.
- Reserve network topology and mesh routing functionality for the Final PoE.

## Smart-X Architecture

The application is organised into three main areas:

1. **Sensor Data Ingestion and Telemetry** - supports sensor registration, file uploads, live readings and telemetry alerts.
2. **Real-Time Command Stream and History** - supports device commands, command history, undo operations and predictive suggestions.
3. **Network Topology and Mesh Routing** - reserved for the Final PoE.

## Technical Concepts Demonstrated

### Generic Telemetry Processing

The `TelemetryPacket<T>` class uses generics to handle different types of sensor readings, including numeric temperature values and Boolean switch states.

### Operator Overloading

The `TelemetryReading` class overloads the `+`, `-`, `>` and `<` operators. This allows telemetry readings to be combined, compared and evaluated using standard C# operators.

### Arrays and Recursion

Arrays are used to store and process data. Recursive methods are used for location validation.

### Jagged Arrays and Historical Telemetry

Jagged arrays can represent historical telemetry readings grouped into batches. These batches can be converted into collections for further processing.

### Collections

The project demonstrates the use of different C# collections:

- `List<T>` for working with groups of telemetry readings.
- `Queue<T>` for processing telemetry in first-in, first-out order.
- `PriorityQueue<TElement, TPriority>` for prioritising alerts.
- `Stack<T>` for managing command history and undo operations.
- `Dictionary<TKey, TValue>` for storing device and command state information.
- `SortedDictionary<TKey, TValue>` for organising historical telemetry by timestamp.
- `HashSet<T>` for tracking unique alert entries and preventing duplicate processing.

### Predictive Action Engine

The predictive action engine uses recorded search and command activity to generate relevant suggestions. Its purpose is to help users repeat common actions more efficiently using previously recorded activity rather than random suggestions.

### IoT Alerts

Temperature readings are evaluated against configured thresholds to determine whether a reading should be classified as Normal, Warning or Critical.

### File Uploads

The application supports uploading sensor-related files, including text files, logs, configuration files and deployment images.

Supported text files can contain sensor details that the API parses and uses to register devices automatically. Other supported file types can be uploaded and saved by the backend.

## Project Structure

- `SmartX.Api` - ASP.NET Core Web API backend. Handles sensor registration, telemetry, alerts, file uploads, device commands and supporting services.
- `SmartX.Client` - Blazor WebAssembly frontend. Provides the user interface, navigation, authentication pages, dashboard, sensor management and command stream.

## How to Run the Project

1. Open the Smart-X solution in Visual Studio 2026.
2. Ensure that the .NET 10 SDK is installed.
3. Configure the `SmartX.Api` and `SmartX.Client` projects as startup projects.
4. Configure the Firebase project and enable Email/Password authentication in the Firebase Console.
5. Ensure that the frontend Firebase configuration matches the Firebase project used by the API.
6. Run both projects.
7. Open the Blazor WebAssembly frontend in your browser.
8. Register an account or sign in to access the application.
9. Use the navigation menu to access the Dashboard, Sensors, Files and Command Stream pages.

**Important:** The API must be running for sensor registration, telemetry processing, file uploads and device commands to work. Sensor and telemetry data are held in memory, so restarting the API clears the current in-memory data.

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

Example command endpoint:

`POST /api/Commands/execute`

OpenAPI documentation is available at:

`/openapi/v1.json`

Authenticated endpoints require a valid Firebase ID token.

## Example Sensor Data

Example sensor registration:

- **Sensor Name:** Greenhouse Sensor
- **Device Identifier:** TEMP-001
- **Deployment Location:** Greenhouse 1
- **Category:** Temperature

Example temperature readings:

- `25°C` - Normal
- `37°C` - Warning
- `45°C` - Critical

These examples illustrate the configured alert thresholds. Actual classifications depend on the thresholds implemented in the API.

## Part 1 Demonstration
https://youtu.be/3DCBFUvEJj4

## Author

Sonali Ramsumar
