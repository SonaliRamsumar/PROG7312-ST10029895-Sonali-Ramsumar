
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using SmartX.Client.Models;

namespace SmartX.Client.Services
{
    // Connects Firebase login status to Blazor's authorization system.
    public class FirebaseAuthenticationStateProvider
        : AuthenticationStateProvider
    {
        private readonly FirebaseAuthService _authService;

        public FirebaseAuthenticationStateProvider(
            FirebaseAuthService authService)
        {
            _authService = authService;
        }

        // Check whether Firebase has a signed-in user.
        public override async Task<AuthenticationState>
            GetAuthenticationStateAsync()
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();

                return CreateAuthenticationState(user);
            }
            catch
            {
                // Treat errors as signed out rather than granting access.
                return CreateAuthenticationState(null);
            }
        }

        // Notify Blazor after login, registration, or logout.
        public async Task RefreshAuthenticationStateAsync()
        {
            AuthenticationState state;

            try
            {
                var user = await _authService.GetCurrentUserAsync();
                state = CreateAuthenticationState(user);
            }
            catch
            {
                state = CreateAuthenticationState(null);
            }

            NotifyAuthenticationStateChanged(
                Task.FromResult(state));
        }

        // Convert the Firebase user into a Blazor identity.
        private static AuthenticationState CreateAuthenticationState(
            FirebaseUserInfo? user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Uid))
            {
                var anonymousIdentity = new ClaimsIdentity();
                var anonymousUser = new ClaimsPrincipal(anonymousIdentity);

                return new AuthenticationState(anonymousUser);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Uid),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(
                claims,
                authenticationType: "Firebase");

            var principal = new ClaimsPrincipal(identity);

            return new AuthenticationState(principal);
        }
    }
}
