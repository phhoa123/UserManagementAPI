using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace UserManagementAPI.Middleware;

/// <summary>Requires a configured bearer token for requests to the API routes.</summary>
public sealed class ApiTokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly byte[] _expectedToken;
    private readonly ILogger<ApiTokenAuthenticationMiddleware> _logger;

    public ApiTokenAuthenticationMiddleware(
        RequestDelegate next,
        IOptions<ApiTokenOptions> options,
        ILogger<ApiTokenAuthenticationMiddleware> logger)
    {
        _next = next;
        _expectedToken = Encoding.UTF8.GetBytes(options.Value.ApiToken);
        _logger = logger;
    }

    /// <summary>Authenticates API requests without logging or exposing the supplied token.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        var authorization = context.Request.Headers.Authorization.ToString();
        var hasBearerToken =
            AuthenticationHeaderValue.TryParse(authorization, out var header) &&
            string.Equals(header.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(header.Parameter);

        var suppliedToken = hasBearerToken
            ? Encoding.UTF8.GetBytes(header!.Parameter!)
            : [];

        if (!hasBearerToken ||
            !CryptographicOperations.FixedTimeEquals(_expectedToken, suppliedToken))
        {
            _logger.LogWarning(
                "Unauthorized API request: {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.WWWAuthenticate = "Bearer";

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "A valid bearer token is required.",
                Instance = context.Request.Path
            };
            problem.Extensions["traceId"] = context.TraceIdentifier;

            await context.Response.WriteAsJsonAsync(
                problem,
                options: null,
                contentType: "application/problem+json",
                cancellationToken: context.RequestAborted);
            return;
        }

        await _next(context);
    }
}
