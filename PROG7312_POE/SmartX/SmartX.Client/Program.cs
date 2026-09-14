using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SmartX.Client;
using SmartX.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddScoped(sp =>
    new HttpClient
    {
        // The frontend will send API requests to our SmartX backend.
        BaseAddress = new Uri("https://localhost:7285/")
    });
builder.Services.AddScoped<SensorApiService>();
await builder.Build().RunAsync();
