using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Skills;

public class SkillGroupRepository : ISkillGroupRepository
{
    private readonly PussyCatsDbContext db;

    public SkillGroupRepository(PussyCatsDbContext db)
    {
        this.db = db;
    }

    /// <summary>
    /// Read-only listing — includes Skills so CompatibilityService can score without N+1
    /// fetches against the catalog.
    /// </summary>
    public async Task<IReadOnlyList<SkillGroup>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.SkillGroups
            .AsNoTracking()
            .Include(group => group.Skills)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Original: PussyCatsApp SkillGroupRepository.GetSkillsGroupByRole — straight predicate
    /// port. CompatibilityService scores against this list per role; the Skills include is the
    /// whole point of the call.
    /// </summary>
    public async Task<IReadOnlyList<SkillGroup>> GetByJobRoleAsync(JobRole jobRole, CancellationToken cancellationToken = default)
    {
        return await db.SkillGroups
            .AsNoTracking()
            .Include(group => group.Skills)
            .Where(group => group.JobRole == jobRole)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
