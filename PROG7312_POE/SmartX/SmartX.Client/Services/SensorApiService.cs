using System.Net.Http.Json;
using SmartX.Client.Models;

namespace SmartX.Client.Services
{
    public class SensorApiService
    {
        private readonly HttpClient _httpClient;

        public SensorApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SensorRegistration>> GetSensorsAsync()
        {
            // Gets all sensors from the backend API.
            return await _httpClient.GetFromJsonAsync<List<SensorRegistration>>(
                "api/Sensors") ?? new List<SensorRegistration>();
        }

        public async Task<SensorRegistration?> AddSensorAsync(
            SensorRegistration sensor)
        {
            // Sends a new sensor registration to the backend.
            var response = await _httpClient.PostAsJsonAsync(
                "api/Sensors",
                sensor);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content
                .ReadFromJsonAsync<SensorRegistration>();
        }
    }
}