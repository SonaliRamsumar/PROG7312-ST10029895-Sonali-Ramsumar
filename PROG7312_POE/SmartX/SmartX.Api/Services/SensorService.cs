
using System.Text.Json;
using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class SensorService
    {
        // Stores registered sensors using their device identifiers.
        private readonly Dictionary<string, SensorRegistration> _sensors =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly object _lock = new();

        // Location of the JSON storage file.
        private readonly string _storageFilePath;

        private int _nextId = 1;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public SensorService(IWebHostEnvironment environment)
        {
            string dataDirectory = Path.Combine(
                environment.ContentRootPath,
                "App_Data");

            _storageFilePath = Path.Combine(
                dataDirectory,
                "sensors.json");

            // Restore previously registered sensors when the API starts.
            LoadSensors();
        }

        // Returns all registered sensors.
        public List<SensorRegistration> GetAll()
        {
            lock (_lock)
            {
                return _sensors.Values
                    .OrderBy(sensor => sensor.Id)
                    .Select(sensor => new SensorRegistration
                    {
                        Id = sensor.Id,
                        SensorName = sensor.SensorName,
                        DeviceIdentifier = sensor.DeviceIdentifier,
                        DeploymentLocation = sensor.DeploymentLocation,
                        Category = sensor.Category
                    })
                    .ToList();
            }
        }

        // Checks whether a device identifier is already registered.
        public bool DeviceExists(string deviceIdentifier)
        {
            lock (_lock)
            {
                return !string.IsNullOrWhiteSpace(deviceIdentifier)
                    && _sensors.ContainsKey(deviceIdentifier.Trim());
            }
        }

        // Registers a sensor and saves it to JSON.
        public SensorRegistration Add(SensorRegistration sensor)
        {
            ArgumentNullException.ThrowIfNull(sensor);

            if (string.IsNullOrWhiteSpace(sensor.DeviceIdentifier))
            {
                throw new ArgumentException(
                    "Device identifier cannot be empty.");
            }

            lock (_lock)
            {
                sensor.DeviceIdentifier = sensor.DeviceIdentifier.Trim();

                if (_sensors.ContainsKey(sensor.DeviceIdentifier))
                {
                    throw new InvalidOperationException(
                        "A sensor with this device identifier is already registered.");
                }

                sensor.Id = _nextId;

                _sensors.Add(sensor.DeviceIdentifier, sensor);
                _nextId++;

                try
                {
                    SaveSensors();
                }
                catch
                {
                    // Undo the in-memory registration if saving fails.
                    _sensors.Remove(sensor.DeviceIdentifier);
                    _nextId--;

                    throw;
                }

                return sensor;
            }
        }

        // Loads saved sensors from the JSON file.
        private void LoadSensors()
        {
            if (!File.Exists(_storageFilePath))
            {
                return;
            }

            try
            {
                string json = File.ReadAllText(_storageFilePath);

                var savedSensors =
                    JsonSerializer.Deserialize<List<SensorRegistration>>(
                        json,
                        _jsonOptions);

                if (savedSensors == null)
                {
                    return;
                }

                foreach (var sensor in savedSensors)
                {
                    if (sensor == null ||
                        string.IsNullOrWhiteSpace(sensor.DeviceIdentifier))
                    {
                        throw new InvalidDataException(
                            "The saved sensor file contains an invalid sensor.");
                    }

                    sensor.DeviceIdentifier =
                        sensor.DeviceIdentifier.Trim();

                    if (_sensors.ContainsKey(sensor.DeviceIdentifier))
                    {
                        throw new InvalidDataException(
                            $"Duplicate device identifier found: {sensor.DeviceIdentifier}");
                    }

                    if (sensor.Id <= 0)
                    {
                        sensor.Id = _nextId;
                    }

                    _sensors.Add(sensor.DeviceIdentifier, sensor);

                    _nextId = Math.Max(
                        _nextId,
                        sensor.Id + 1);
                }
            }
            catch (JsonException exception)
            {
                throw new InvalidOperationException(
                    "The saved sensor JSON file could not be read. " +
                    "Check App_Data/sensors.json.",
                    exception);
            }
        }

        // Saves all registered sensors to the JSON file.
        private void SaveSensors()
        {
            string? temporaryFile = null;

            try
            {
                string directory =
                    Path.GetDirectoryName(_storageFilePath)!;

                Directory.CreateDirectory(directory);

                temporaryFile = _storageFilePath + ".tmp";

                var sensorsToSave = _sensors.Values
                    .OrderBy(sensor => sensor.Id)
                    .ToList();

                string json = JsonSerializer.Serialize(
                    sensorsToSave,
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
