using UserManagementAPI.Contracts;
using UserManagementAPI.Models;

namespace UserManagementAPI.Services;

/// <summary>Stores users in an in-memory list for the lifetime of the application.</summary>
public sealed class UserService : IUserService
{
    private readonly List<User> _users = [];
    private readonly Lock _usersLock = new();
    private int _nextId = 1;

    public IReadOnlyList<UserResponse> GetAll(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_usersLock)
        {
            return _users.Select(ToResponse).ToArray();
        }
    }

    public UserResponse? GetById(int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_usersLock)
        {
            var user = _users.Find(user => user.Id == id);
            return user is null ? null : ToResponse(user);
        }
    }

    public UserResponse Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_usersLock)
        {
            var user = new User
            {
                Id = _nextId++,
                Name = request.Name,
                Email = request.Email,
                Department = request.Department
            };

            _users.Add(user);
            return ToResponse(user);
        }
    }

    public UserResponse? Update(int id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_usersLock)
        {
            var index = _users.FindIndex(user => user.Id == id);
            if (index < 0)
            {
                return null;
            }

            var updatedUser = new User
            {
                Id = id,
                Name = request.Name,
                Email = request.Email,
                Department = request.Department
            };

            _users[index] = updatedUser;
            return ToResponse(updatedUser);
        }
    }

    public bool Delete(int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_usersLock)
        {
            var index = _users.FindIndex(user => user.Id == id);
            if (index < 0)
            {
                return false;
            }

            _users.RemoveAt(index);
            return true;
        }
    }

    private static UserResponse ToResponse(User user) =>
        new(user.Id, user.Name, user.Email, user.Department);
}
