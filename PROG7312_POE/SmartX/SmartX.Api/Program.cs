using SmartX.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<SensorService>();
builder.Services.AddSingleton<TelemetryService>();
builder.Services.AddSingleton<LocationValidationService>();
builder.Services.AddSingleton<FileUploadService>();
builder.Services.AddSingleton<TelemetryHistoryService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        // Allows our Blazor frontend to call this API while developing.
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
