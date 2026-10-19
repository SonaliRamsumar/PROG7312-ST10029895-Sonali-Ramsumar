
using SmartX.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register the application services.
builder.Services.AddSingleton<SensorService>();
builder.Services.AddSingleton<TelemetryService>();
builder.Services.AddSingleton<LocationValidationService>();
builder.Services.AddSingleton<FileUploadService>();
builder.Services.AddSingleton<TelemetryHistoryService>();

// Register the telemetry queue for FIFO processing.
builder.Services.AddSingleton<TelemetryQueue<double>>();

// Register the priority queue for urgent telemetry alerts.
builder.Services.AddSingleton<AlertPriorityQueue>();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
