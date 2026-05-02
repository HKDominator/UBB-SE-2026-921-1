using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Skills;

public class UserSkillRepository : IUserSkillRepository
{
    private readonly PussyCatsDbContext db;

    public UserSkillRepository(PussyCatsDbContext db)
    {
        this.db = db;
    }

    /// <summary>
    /// Tracked single-row lookup keyed by the (UserId, SkillId) composite. Includes Skill so
    /// the caller can render the catalog name without a second query.
    /// </summary>
    public async Task<UserSkill?> GetAsync(int userId, int skillId, CancellationToken ct = default)
    {
        return await db.UserSkills
            .Include(s => s.Skill)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.SkillId == skillId, ct)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Read-only listing of every claimed skill for a user. Includes Skill for catalog name.
    /// </summary>
    public async Task<IReadOnlyList<UserSkill>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await db.UserSkills
            .AsNoTracking()
            .Include(s => s.Skill)
            .Where(s => s.UserId == userId)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Original: PussyCatsApp UserSkillRepository.GetVerifiedSkillsByUserId — the original SQL
    /// only filtered on userId and *implied* IsVerified by reading the SKILLS table (the legacy
    /// schema only stored verified rows there). The new model keeps unverified self-claims in
    /// the same UserSkill table, so the LINQ predicate also requires IsVerified = true and
    /// AchievedDate IS NOT NULL — both must hold for a skill to count as "verified", per the
    /// AchievedDate XML doc note on UserSkill.
    /// </summary>
    public async Task<IReadOnlyList<UserSkill>> GetVerifiedByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await db.UserSkills
            .AsNoTracking()
            .Include(s => s.Skill)
            .Where(s => s.UserId == userId && s.IsVerified && s.AchievedDate != null)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<UserSkill> AddAsync(UserSkill userSkill, CancellationToken ct = default)
    {
        db.UserSkills.Add(userSkill);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
        return userSkill;
    }

    public async Task UpdateAsync(UserSkill userSkill, CancellationToken ct = default)
    {
        db.UserSkills.Update(userSkill);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Targeted column update through the change tracker — same pattern as
    /// SkillTestRepository.UpdateScoreAsync.
    /// </summary>
    public async Task UpdateScoreAsync(int userId, int skillId, int score, CancellationToken ct = default)
    {
        var userSkill = await db.UserSkills.FindAsync(new object?[] { userId, skillId }, ct).ConfigureAwait(false);
        if (userSkill is null)
        {
            return;
        }
        userSkill.Score = score;
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int userId, int skillId, CancellationToken ct = default)
    {
        var userSkill = await db.UserSkills.FindAsync(new object?[] { userId, skillId }, ct).ConfigureAwait(false);
        if (userSkill is null)
        {
            return;
        }
        db.UserSkills.Remove(userSkill);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
