using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.SkillTests;

public interface ISkillTestRepository
{
    Task<SkillTest?> GetByIdAsync(int skillTestId, CancellationToken ct = default);

    Task<IReadOnlyList<SkillTest>> GetByUserIdAsync(int userId, CancellationToken ct = default);

    Task<SkillTest> AddAsync(SkillTest skillTest, CancellationToken ct = default);

    Task UpdateScoreAsync(int skillTestId, int score, CancellationToken ct = default);

    Task UpdateAchievedDateAsync(int skillTestId, DateOnly achievedDate, CancellationToken ct = default);

    Task RemoveAsync(int skillTestId, CancellationToken ct = default);
}
