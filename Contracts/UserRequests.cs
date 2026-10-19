using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.Contracts;

/// <summary>Data required to create a user.</summary>
public sealed record CreateUserRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    [Required, EmailAddress, StringLength(254, MinimumLength = 3)]
    public required string Email { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string Department { get; init; }
}

/// <summary>Data used to replace an existing user's details.</summary>
public sealed record UpdateUserRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    [Required, EmailAddress, StringLength(254, MinimumLength = 3)]
    public required string Email { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string Department { get; init; }
}

/// <summary>User details returned by the API.</summary>
public sealed record UserResponse(
    int Id,
    string Name,
    string Email,
    string Department);
