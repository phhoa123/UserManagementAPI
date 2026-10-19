using UserManagementAPI.Contracts;

namespace UserManagementAPI.Services;

/// <summary>Provides in-memory CRUD operations for users.</summary>
public interface IUserService
{
    IReadOnlyList<UserResponse> GetAll(CancellationToken cancellationToken);

    UserResponse? GetById(int id, CancellationToken cancellationToken);

    UserResponse Create(CreateUserRequest request, CancellationToken cancellationToken);

    UserResponse? Update(int id, UpdateUserRequest request, CancellationToken cancellationToken);

    bool Delete(int id, CancellationToken cancellationToken);
}
