
using Microsoft.JSInterop;
using SmartX.Client.Models;

namespace SmartX.Client.Services
{
    public class FirebaseAuthService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private IJSObjectReference? _firebaseModule;

        public FirebaseAuthService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        // Load the JavaScript module only when it is needed.
        private async Task<IJSObjectReference> GetModuleAsync()
        {
            if (_firebaseModule == null)
            {
                _firebaseModule =
                    await _jsRuntime.InvokeAsync<IJSObjectReference>(
                        "import",
                        "./js/firebase-auth.js");
            }

            return _firebaseModule;
        }

        // Register a new user with Firebase.
        public async Task<FirebaseUserInfo> RegisterAsync(
            string email,
            string password)
        {
            var module = await GetModuleAsync();

            return await module.InvokeAsync<FirebaseUserInfo>(
                "register",
                email,
                password);
        }

        // Sign in an existing Firebase user.
        public async Task<FirebaseUserInfo> SignInAsync(
            string email,
            string password)
        {
            var module = await GetModuleAsync();

            return await module.InvokeAsync<FirebaseUserInfo>(
                "signIn",
                email,
                password);
        }

        // Sign out the current user.
        public async Task SignOutAsync()
        {
            var module = await GetModuleAsync();

            await module.InvokeVoidAsync("signOut");
        }

        // Get the currently signed-in Firebase user.
        public async Task<FirebaseUserInfo?> GetCurrentUserAsync()
        {
            var module = await GetModuleAsync();

            return await module.InvokeAsync<FirebaseUserInfo?>(
                "getCurrentUser");
        }

        // Get the Firebase ID token for API authentication.
        public async Task<string?> GetIdTokenAsync()
        {
            var module = await GetModuleAsync();

            return await module.InvokeAsync<string?>(
                "getIdToken");
        }

        // Release the JavaScript module when the service is disposed.
        public async ValueTask DisposeAsync()
        {
            if (_firebaseModule != null)
            {
                try
                {
                    await _firebaseModule.DisposeAsync();
                }
                catch (JSDisconnectedException)
                {
                    // The browser may already have disconnected.
                }
            }
        }
    }
}
