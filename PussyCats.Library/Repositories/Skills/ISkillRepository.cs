using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.Skills;

public interface ISkillRepository
{
    Task<Skill?> GetByIdAsync(int skillId, CancellationToken ct = default);

    Task<IReadOnlyList<Skill>> GetAllAsync(CancellationToken ct = default);

    Task<Skill> AddAsync(Skill skill, CancellationToken ct = default);

    Task UpdateAsync(Skill skill, CancellationToken ct = default);

    Task RemoveAsync(int skillId, CancellationToken ct = default);
}
