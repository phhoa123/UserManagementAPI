namespace UserManagementAPI.Middleware;

/// <summary>Converts unhandled request exceptions into a generic JSON error response.</summary>
public sealed class ApiExceptionMiddleware(
    RequestDelegate next,
    ILogger<ApiExceptionMiddleware> logger)
{
    /// <summary>Runs the remaining pipeline and handles unhandled exceptions.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}. TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(
                new { error = "Internal server error." },
                options: null,
                contentType: "application/json",
                cancellationToken: context.RequestAborted);
        }
    }
}
