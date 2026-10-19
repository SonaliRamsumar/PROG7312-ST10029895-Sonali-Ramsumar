
using SmartX.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add controllers.
builder.Services.AddControllers();

// Register the application services.
builder.Services.AddSingleton<SensorService>();
builder.Services.AddSingleton<TelemetryService>();
builder.Services.AddSingleton<LocationValidationService>();
builder.Services.AddSingleton<FileUploadService>();
builder.Services.AddSingleton<TelemetryHistoryService>();

// Register telemetry processing structures.
builder.Services.AddSingleton<TelemetryQueue<double>>();
builder.Services.AddSingleton<AlertPriorityQueue>();

// Register manual command history and current device states.
builder.Services.AddSingleton<CommandHistoryService>();
builder.Services.AddSingleton<CommandStateService>();

// Store actual searches and command activity for the predictive engine.
builder.Services.AddSingleton<UserActivityService>();

// Allow the Blazor frontend to communicate with the API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors("AllowClient");

// Expose OpenAPI documentation in development.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
