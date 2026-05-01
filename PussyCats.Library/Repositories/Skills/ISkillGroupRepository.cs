using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.Repositories.Skills;

public interface ISkillGroupRepository
{
    Task<IReadOnlyList<SkillGroup>> GetAllAsync(CancellationToken ct = default);

    Task<IReadOnlyList<SkillGroup>> GetByJobRoleAsync(JobRole jobRole, CancellationToken ct = default);
}
