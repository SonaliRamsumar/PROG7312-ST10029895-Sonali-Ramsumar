
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SmartX.Client;
using SmartX.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure communication with the Smart-X API.
builder.Services.AddScoped(sp =>
    new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7285/")
    });

// Register existing application services.
builder.Services.AddScoped<SensorApiService>();
builder.Services.AddScoped<CommandStreamApiService>();

// Register Firebase authentication.
builder.Services.AddScoped<FirebaseAuthService>();

await builder.Build().RunAsync();
