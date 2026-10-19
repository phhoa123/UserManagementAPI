namespace UserManagementAPI.Models;

/// <summary>Represents a user stored by the API.</summary>
public sealed class User
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required string Email { get; init; }

    public required string Department { get; init; }
}
