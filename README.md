# Smart-X IoT Management System

Smart-X is a .NET 10 IoT management system used to register sensors, monitor telemetry readings, upload sensor-related files and identify unusual sensor conditions.

## Technologies Used

- .NET 10
- ASP.NET Core Web API
- Blazor WebAssembly
- C#
- GitHub

## Main Features

- Register IoT sensors
- View registered sensors
- Monitor simulated telemetry readings
- Display normal, warning and critical alert states
- Upload sensor configuration files, logs and deployment images
- Process telemetry using generics
- Use operator overloading for telemetry readings
- Use arrays, recursion and collections
- Process historical telemetry using jagged arrays

## Smart-X Architecture

The startup page displays the three Smart-X architecture areas:

- Sensor Data Ingestion and Telemetry - available in Part 1
- Real-Time Command Stream and History - planned for Part 2
- Network Topology and Mesh Routing - planned for the Final PoE

## Technical Concepts Demonstrated

### Generic Telemetry Processing

The project uses a generic `TelemetryPacket<T>` class so different sensor values, such as temperature values and switch states, can be handled using the same structure.

### Operator Overloading

The `TelemetryReading` class overloads the `+` operator so two telemetry readings can be combined using normal C# syntax.

### Arrays and Recursion

Allowed deployment locations are stored in an array and checked using a recursive validation method.

### Jagged Arrays and Historical Telemetry

Historical telemetry can be received in batches using a jagged array and converted into a collection for easier processing.

### Collections

Telemetry arrays can be converted into `List<double>` collections for easier processing.

### IoT Alerts

The dashboard uses simple temperature thresholds to display Normal, Warning and Critical states.

### File Uploads

Users can upload sensor-related files such as logs, configuration files and deployment images.

## Project Structure

- `SmartX.Api` - ASP.NET Core Web API backend
- `SmartX.Client` - Blazor WebAssembly frontend

## How to Run the Project

1. Open the SmartX solution in Visual Studio 2026.
2. Make sure .NET 10 is installed.
3. Configure both `SmartX.Api` and `SmartX.Client` as startup projects.
4. Run the solution.
5. Keep both projects running while using the application.
6. Open the Smart-X frontend in the browser.
7. Use the navigation menu to access the Dashboard, Sensors and Files pages.

## API

During development, the backend API runs locally using HTTPS.

Example API address:

`https://localhost:7285/`

Example sensor endpoint:

`/api/Sensors`

OpenAPI information can be viewed at:

`/openapi/v1.json`

## Example Sensor Data

Example sensor registration:

- Sensor Name: `Greenhouse Sensor`
- Device Identifier: `TEMP-001`
- Deployment Location: `Greenhouse 1`
- Category: `Temperature`

Example dashboard readings:

- `25` = Normal
- `37` = Warning
- `45` = Critical

##YouTube video of part 1

https://youtu.be/3DCBFUvEJj4
## Author

Sonali Ramsumar
