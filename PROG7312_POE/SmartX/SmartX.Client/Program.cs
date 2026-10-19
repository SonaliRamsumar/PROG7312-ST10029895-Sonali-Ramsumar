
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SmartX.Client;
using SmartX.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure the HTTP client to communicate with the SmartX API.
builder.Services.AddScoped(sp =>
    new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7285/")
    });

// Register the services used by the frontend.
builder.Services.AddScoped<SensorApiService>();
builder.Services.AddScoped<CommandStreamApiService>();

await builder.Build().RunAsync();
