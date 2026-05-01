using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.Skills;

public interface IUserSkillRepository
{
    Task<UserSkill?> GetAsync(int userId, int skillId, CancellationToken ct = default);

    Task<IReadOnlyList<UserSkill>> GetByUserIdAsync(int userId, CancellationToken ct = default);

    Task<IReadOnlyList<UserSkill>> GetVerifiedByUserIdAsync(int userId, CancellationToken ct = default);

    Task<UserSkill> AddAsync(UserSkill userSkill, CancellationToken ct = default);

    Task UpdateAsync(UserSkill userSkill, CancellationToken ct = default);

    Task UpdateScoreAsync(int userId, int skillId, int score, CancellationToken ct = default);

    Task RemoveAsync(int userId, int skillId, CancellationToken ct = default);
}
