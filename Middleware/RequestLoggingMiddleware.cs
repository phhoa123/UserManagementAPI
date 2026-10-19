namespace UserManagementAPI.Middleware;

/// <summary>Logs the HTTP method, request path, and final response status code.</summary>
public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    /// <summary>Logs a request and its final response status.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path;

        logger.LogInformation("HTTP request started: {Method} {Path}", method, path);

        try
        {
            await next(context);
        }
        finally
        {
            logger.LogInformation(
                "HTTP request completed: {Method} {Path} responded {StatusCode}",
                method,
                path,
                context.Response.StatusCode);
        }
    }
}
