using UserManagementAPI.Services;
using UserManagementAPI.Middleware;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services
    .AddOptions<ApiTokenOptions>()
    .BindConfiguration(ApiTokenOptions.SectionName)
    .Validate(
        options => options.ApiToken.Length >= 32,
        "Auth:ApiToken must contain at least 32 characters.")
    .ValidateOnStart();

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseMiddleware<ApiTokenAuthenticationMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

app.Run();

public partial class Program
{
}
