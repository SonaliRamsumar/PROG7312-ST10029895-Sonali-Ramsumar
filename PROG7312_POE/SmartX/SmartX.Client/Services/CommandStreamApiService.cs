
using System.Net.Http.Json;
using SmartX.Client.Models;

namespace SmartX.Client.Services
{
    public class CommandStreamApiService
    {
        private readonly HttpClient _httpClient;

        public CommandStreamApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Retrieve registered devices, command states, and command history.
        public async Task<CommandStreamResponse?> GetCommandStreamAsync()
        {
            return await _httpClient.GetFromJsonAsync<CommandStreamResponse>(
                "api/Commands/stream");
        }

        // Send a request to switch a relay On or Off.
        public async Task<HttpResponseMessage> ExecuteRelayCommandAsync(
            string deviceIdentifier,
            string newValue)
        {
            var request = new ManualCommandRequest
            {
                DeviceIdentifier = deviceIdentifier,
                CommandName = "SetRelayState",
                NewValue = newValue
            };

            return await _httpClient.PostAsJsonAsync(
                "api/Commands/execute",
                request);
        }

        // Ask the API to undo the most recent command.
        public async Task<HttpResponseMessage> UndoLastCommandAsync()
        {
            return await _httpClient.PostAsync(
                "api/Commands/undo",
                null);
        }
    }
}
