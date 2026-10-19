
using System.Net.Http.Headers;

namespace SmartX.Client.Services
{
    // Adds the signed-in Firebase user's ID token to API requests.
    public class FirebaseAuthMessageHandler : DelegatingHandler
    {
        private readonly FirebaseAuthService _authService;

        public FirebaseAuthMessageHandler(
            FirebaseAuthService authService)
        {
            _authService = authService;
        }

        // Runs automatically before each API request is sent.
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Retrieve the current user's Firebase ID token.
            var token = await _authService.GetIdTokenAsync();

            // Attach the token when a user is signed in.
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            // Continue sending the request to the API.
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
