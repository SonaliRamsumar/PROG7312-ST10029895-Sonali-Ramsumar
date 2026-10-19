
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SmartX.Client;
using SmartX.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register Firebase authentication.
builder.Services.AddScoped<FirebaseAuthService>();

// Connect Firebase authentication to Blazor's authorization system.
builder.Services.AddScoped<FirebaseAuthenticationStateProvider>();

builder.Services.AddScoped<AuthenticationStateProvider>(
    serviceProvider =>
        serviceProvider.GetRequiredService<
            FirebaseAuthenticationStateProvider>());

// Register the handler that attaches Firebase tokens to API requests.
builder.Services.AddScoped<FirebaseAuthMessageHandler>();

// Configure the API HttpClient to use the Firebase token handler.
builder.Services.AddScoped(serviceProvider =>
{
    var authService =
        serviceProvider.GetRequiredService<FirebaseAuthService>();

    var handler = new FirebaseAuthMessageHandler(authService)
    {
        InnerHandler = new HttpClientHandler()
    };

    return new HttpClient(handler)
    {
        BaseAddress = new Uri("https://localhost:7285/")
    };
});

// Register the existing application services.
builder.Services.AddScoped<SensorApiService>();
builder.Services.AddScoped<CommandStreamApiService>();

// Enable authorization support in Blazor WebAssembly.
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
