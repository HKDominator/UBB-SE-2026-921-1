using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Skills;

public class SkillRepository : ISkillRepository
{
    private readonly PussyCatsDbContext db;

    public SkillRepository(PussyCatsDbContext db)
    {
        this.db = db;
    }

    public async Task<Skill?> GetByIdAsync(int skillId, CancellationToken ct = default)
    {
        return await db.Skills
            .FirstOrDefaultAsync(s => s.SkillId == skillId, ct)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Catalog listing — read-only, ordered by name for stable UI rendering.
    /// </summary>
    public async Task<IReadOnlyList<Skill>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Skills
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<Skill> AddAsync(Skill skill, CancellationToken ct = default)
    {
        db.Skills.Add(skill);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
        return skill;
    }

    public async Task UpdateAsync(Skill skill, CancellationToken ct = default)
    {
        db.Skills.Update(skill);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int skillId, CancellationToken ct = default)
    {
        var skill = await db.Skills.FindAsync(new object?[] { skillId }, ct).ConfigureAwait(false);
        if (skill is null)
        {
            return;
        }
        db.Skills.Remove(skill);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
