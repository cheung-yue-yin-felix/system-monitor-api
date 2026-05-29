using System.Text.Json;
using SystemMonitorApi.Authentication;
using SystemMonitorApi.Models;
using SystemMonitorApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IRegistryReader, RegistryReader>();
builder.Services.AddSingleton<ISensorMapper, SensorMapper>();
builder.Services.AddSingleton<IRegistryPoller, RegistryPoller>();
builder.Services.Configure<SensorMappingOptions>(
    builder.Configuration.GetSection(SensorMappingOptions.SectionName));
builder.Services.Configure<SensorRegistryOptions>(
    builder.Configuration.GetSection(SensorRegistryOptions.SectionName));

// CORS for React dev server and any production origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                  "https://cheung-yue-yin-felix.github.io",
                  "https://system-monitor",
                  "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseMiddleware<ApiKeyMiddleware>();

// Raw registry values (flat array)
app.MapGet("/api/sensors", (IRegistryReader registryReader) =>
    {
        var sensors = registryReader.ReadSensorValues();
        return Results.Ok(sensors);
    })
    .WithName("GetSensors");

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
};

// Structured metrics snapshot
app.MapGet("/api/metrics", (IRegistryReader registryReader, ISensorMapper mapper) =>
    {
        var raw = registryReader.ReadSensorValues();
        var metrics = mapper.MapToStructured(raw);
        return Results.Json(metrics, jsonOptions);
    })
    .WithName("GetMetrics");

// Server-Sent Events stream of metrics updates
app.MapGet("/api/metrics/stream", async (
    HttpContext context,
    IRegistryPoller poller,
    CancellationToken cancellationToken) =>
{
    context.Response.Headers.Append("Content-Type", "text/event-stream");
    context.Response.Headers.Append("Cache-Control", "no-cache");
    context.Response.Headers.Append("Connection", "keep-alive");

    await foreach (var metrics in poller.StreamAsync(TimeSpan.FromSeconds(1), cancellationToken))
    {
        var json = JsonSerializer.Serialize(metrics, jsonOptions);
        await context.Response.WriteAsync($"data: {json}\n\n", cancellationToken);
        await context.Response.Body.FlushAsync(cancellationToken);
    }
})
.WithName("GetMetricsStream");

app.Run();
