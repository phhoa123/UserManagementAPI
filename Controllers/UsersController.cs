using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Contracts;
using UserManagementAPI.Services;

namespace UserManagementAPI.Controllers;

/// <summary>Provides CRUD endpoints for managing users.</summary>
[ApiController]
[Route("api/users")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>Gets all users currently stored in memory.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<UserResponse>> GetAll(CancellationToken cancellationToken) =>
        Ok(userService.GetAll(cancellationToken));

    /// <summary>Gets a user by ID.</summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserResponse> GetById(int id, CancellationToken cancellationToken) =>
        userService.GetById(id, cancellationToken) is { } user
            ? Ok(user)
            : NotFound();

    /// <summary>Creates a new user.</summary>
    /// <param name="request">The user details to create.</param>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<UserResponse> Create(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = userService.Create(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    /// <summary>Replaces the details of an existing user.</summary>
    /// <param name="id">The ID of the user to update.</param>
    /// <param name="request">The replacement user details.</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<UserResponse> Update(
        int id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = userService.Update(id, request, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>Deletes a user by ID.</summary>
    /// <param name="id">The ID of the user to delete.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id, CancellationToken cancellationToken) =>
        userService.Delete(id, cancellationToken)
            ? NoContent()
            : NotFound();
}
