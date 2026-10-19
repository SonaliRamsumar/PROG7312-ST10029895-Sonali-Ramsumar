
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using SmartX.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Register API controllers.
builder.Services.AddControllers();

// Register the main application services.
builder.Services.AddSingleton<SensorService>();
builder.Services.AddSingleton<TelemetryService>();
builder.Services.AddSingleton<LocationValidationService>();
builder.Services.AddSingleton<FileUploadService>();
builder.Services.AddSingleton<TelemetryHistoryService>();
builder.Services.AddSingleton<LiveTelemetryService>();

// Register the telemetry queue and alert priority queue.
builder.Services.AddSingleton<TelemetryQueue<double>>();
builder.Services.AddSingleton<AlertPriorityQueue>();

// Register manual command history and device command states.
builder.Services.AddSingleton<CommandHistoryService>();
builder.Services.AddSingleton<CommandStateService>();

// Store actual searches and commands for the predictive action engine.
builder.Services.AddSingleton<UserActivityService>();

// Configure Firebase authentication.
const string firebaseProjectId = "smart-x-iot-management";

var firebaseIssuer =
    $"https://securetoken.google.com/{firebaseProjectId}";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Identify the Firebase project that issues the tokens.
        options.Authority = firebaseIssuer;

        // Firebase tokens must be validated over HTTPS.
        options.RequireHttpsMetadata = true;

        // Validate the token issuer, audience, expiry and signature.
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = firebaseIssuer,

                ValidateAudience = true,
                ValidAudience = firebaseProjectId,

                ValidateLifetime = true,

                // Allow a small difference between server and client clocks.
                ClockSkew = TimeSpan.FromMinutes(2)
            };
    });

// Require authentication for API endpoints by default.
// Individual endpoints can explicitly allow anonymous access if required.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Allow the Blazor WebAssembly client to communicate with the API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register OpenAPI support.
builder.Services.AddOpenApi();

var app = builder.Build();

// Expose OpenAPI documentation during development.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseCors("AllowClient");

// Authenticate incoming requests before checking authorization.
app.UseAuthentication();
app.UseAuthorization();

// Map API controllers.
app.MapControllers();

app.Run();
