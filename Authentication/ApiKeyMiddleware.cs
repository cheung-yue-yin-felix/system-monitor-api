namespace SystemMonitorApi.Authentication;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private const string ApiKeyHeaderName = "X-API-Key";

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Allow CORS preflight and Swagger/OpenAPI without API key
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        if (context.Request.Method == "OPTIONS" ||
            (context.Request.Method == "GET" && (path.Contains("openapi") || path.Contains("swagger"))))
        {
            await _next(context);
            return;
        }

        // Support header or query string (query string needed for browser EventSource)
        var extractedApiKey = context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var headerKey)
            ? headerKey.ToString()
            : context.Request.Query["apiKey"].ToString();

        if (string.IsNullOrWhiteSpace(extractedApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API Key is missing. Pass via X-API-Key header or ?apiKey= query parameter.");
            return;
        }

        var configuredApiKey = _configuration.GetValue<string>("ApiKey");

        if (string.IsNullOrWhiteSpace(configuredApiKey) || !configuredApiKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid API Key.");
            return;
        }

        await _next(context);
    }
}
