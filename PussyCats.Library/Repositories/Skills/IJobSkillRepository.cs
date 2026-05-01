using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.Skills;

public interface IJobSkillRepository
{
    Task<JobSkill?> GetAsync(int jobId, int skillId, CancellationToken ct = default);

    Task<IReadOnlyList<JobSkill>> GetByJobIdAsync(int jobId, CancellationToken ct = default);

    Task<JobSkill> AddAsync(JobSkill jobSkill, CancellationToken ct = default);

    Task UpdateAsync(JobSkill jobSkill, CancellationToken ct = default);

    Task RemoveAsync(int jobId, int skillId, CancellationToken ct = default);
}
