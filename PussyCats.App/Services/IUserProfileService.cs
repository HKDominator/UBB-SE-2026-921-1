using PussyCats.Library.Domain;

namespace PussyCats.App.Services;

/// <summary>Candidate profile read/write, including CV text generation and level recalculation.</summary>
public interface IUserProfileService
{
    Task<User?> GetProfileAsync(int userId, CancellationToken ct = default);

    Task<IReadOnlyList<SkillTest>> GetSkillTestsForUserAsync(int userId, CancellationToken ct = default);

    Task<bool> IsProfileAvailableAsync(int userId, CancellationToken ct = default);

    Task UpdateAccountStatusAsync(int userId, bool isActive, CancellationToken ct = default);

    Task UpdateProfilePicturePathAsync(int userId, string newPath, CancellationToken ct = default);

    Task RemoveProfilePicturePathAsync(int userId, CancellationToken ct = default);

    string GenerateParsedCvText(User user);

    Task SaveAsync(int userId, User user, CancellationToken ct = default);

    Task<int> RecalculateLevelAsync(User user, CancellationToken ct = default);
}
