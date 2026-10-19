
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

        // Retrieve devices, command states, and command history.
        public async Task<CommandStreamResponse?> GetCommandStreamAsync()
        {
            return await _httpClient.GetFromJsonAsync<CommandStreamResponse>(
                "api/Commands/stream");
        }

        // Send a relay command to the backend.
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

        // Undo the most recently executed command.
        public async Task<HttpResponseMessage> UndoLastCommandAsync()
        {
            return await _httpClient.PostAsync(
                "api/Commands/undo",
                null);
        }

        // Send the search and filter choices to the activity tracker.
        public async Task<HttpResponseMessage> RecordSearchAsync(
            string query,
            string category,
            string stateFilter)
        {
            var request = new SearchActivityRequest
            {
                Query = query,
                Category = category,
                StateFilter = stateFilter
            };

            return await _httpClient.PostAsJsonAsync(
                "api/Activity/search",
                request);
        }
    }
}
