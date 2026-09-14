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

## Project Structure

- `SmartX.Api` - Backend Web API
- `SmartX.Client` - Blazor WebAssembly frontend

## How to Run the Project

1. Open the SmartX solution in Visual Studio 2026.
2. Make sure both `SmartX.Api` and `SmartX.Client` are set as startup projects.
3. Run the solution.
4. Open the Smart-X frontend in the browser.
5. Use the navigation menu to access the Dashboard, Sensors and Files pages.

## API

The backend API runs locally using HTTPS.

Example sensor endpoint:

`/api/Sensors`

## Author

Sonali Ramsumar
