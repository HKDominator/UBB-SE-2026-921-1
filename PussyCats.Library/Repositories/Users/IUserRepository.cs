using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int userId, CancellationToken ct = default);

    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default);

    Task<User> AddAsync(User user, CancellationToken ct = default);

    Task UpdateAsync(User user, CancellationToken ct = default);

    Task RemoveAsync(int userId, CancellationToken ct = default);

    Task UpdateActiveAccountAsync(int userId, bool isActive, CancellationToken ct = default);

    Task UpdateProfilePicturePathAsync(int userId, string profilePicturePath, CancellationToken ct = default);

    Task TouchLastUpdatedAsync(int userId, CancellationToken ct = default);
}
