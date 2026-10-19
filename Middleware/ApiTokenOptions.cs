namespace UserManagementAPI.Middleware;

/// <summary>Configuration for the shared API bearer token.</summary>
public sealed class ApiTokenOptions
{
    public const string SectionName = "Auth";

    public string ApiToken { get; set; } = string.Empty;
}
