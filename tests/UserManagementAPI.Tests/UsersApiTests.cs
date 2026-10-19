using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using UserManagementAPI.Contracts;
using UserManagementAPI.Middleware;

namespace UserManagementAPI.Tests;

public sealed class UsersApiTests
{
    private const string TestToken = "test-token-for-user-management-api-123456";

    [Fact]
    public async Task CrudEndpoints_CreateReadUpdateAndDeleteUser()
    {
        using var factory = CreateFactory();
        using var client = CreateAuthorizedClient(factory);

        var createResponse = await client.PostAsJsonAsync(
            "/api/users",
            new
            {
                Name = "An Nguyen",
                Email = "an.nguyen@example.com",
                Department = "Engineering"
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(createdUser);
        Assert.Equal($"/api/users/{createdUser.Id}", createResponse.Headers.Location?.AbsolutePath);

        var getResponse = await client.GetAsync($"/api/users/{createdUser.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetchedUser = await getResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.Equal("an.nguyen@example.com", fetchedUser?.Email);

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/users/{createdUser.Id}",
            new
            {
                Name = "An Nguyen",
                Email = "an.nguyen@example.com",
                Department = "Platform Engineering"
            });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updatedUser = await updateResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.Equal("Platform Engineering", updatedUser?.Department);

        var deleteResponse = await client.DeleteAsync($"/api/users/{createdUser.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var missingUserResponse = await client.GetAsync($"/api/users/{createdUser.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missingUserResponse.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithInvalidEmail_ReturnsBadRequest()
    {
        using var factory = CreateFactory();
        using var client = CreateAuthorizedClient(factory);

        var response = await client.PostAsJsonAsync(
            "/api/users",
            new
            {
                Name = "An Nguyen",
                Email = "not-an-email",
                Department = "Engineering"
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ApiRequest_WithoutBearerToken_ReturnsUnauthorized()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("Bearer", response.Headers.WwwAuthenticate.Single().Scheme);
    }

    [Fact]
    public async Task ApiExceptionMiddleware_ReturnsGenericJsonError()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/users";
        context.Response.Body = new MemoryStream();
        var middleware = new ApiExceptionMiddleware(
            _ => throw new InvalidOperationException("Sensitive internal detail."),
            NullLogger<ApiExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        using var error = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(
            "Internal server error.",
            error.RootElement.GetProperty("error").GetString());
        Assert.DoesNotContain("Sensitive internal detail.", error.RootElement.ToString());
    }

    private static WebApplicationFactory<Program> CreateFactory() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Auth:ApiToken"] = TestToken
                })));

    private static HttpClient CreateAuthorizedClient(WebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestToken);
        return client;
    }
}
