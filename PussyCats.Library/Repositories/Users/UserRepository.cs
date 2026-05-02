using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Users;

public class UserRepository : IUserRepository
{
    private readonly PussyCatsDbContext db;

    public UserRepository(PussyCatsDbContext db)
    {
        this.db = db;
    }

    /// <summary>
    /// Includes WorkExperiences, Projects, ExtraCurricularActivities, Skills.Skill, and
    /// PersonalityResult.TraitScores so callers receive a fully populated profile in one round trip.
    /// Tracked because the typical caller (UserService.UpdateProfile) mutates the entity.
    /// </summary>
    public async Task<User?> GetByIdAsync(int userId, CancellationToken ct = default)
    {
        return await db.Users
            .Include(u => u.WorkExperiences)
            .Include(u => u.Projects)
            .Include(u => u.ExtraCurricularActivities)
            .Include(u => u.Skills).ThenInclude(s => s.Skill)
            .Include(u => u.PersonalityResult)!.ThenInclude(r => r!.TraitScores)
            .FirstOrDefaultAsync(u => u.UserId == userId, ct)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Read-only listing — no Includes to keep the query light. Callers that need detail load
    /// it through GetByIdAsync.
    /// </summary>
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Users
            .AsNoTracking()
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        if (user.CreatedAt == default)
        {
            user.CreatedAt = now;
        }
        user.LastUpdated = now;

        db.Users.Add(user);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        user.LastUpdated = DateTime.UtcNow;
        db.Users.Update(user);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int userId, CancellationToken ct = default)
    {
        var user = await db.Users.FindAsync(new object?[] { userId }, ct).ConfigureAwait(false);
        if (user is null)
        {
            return;
        }
        db.Users.Remove(user);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Soft-delete primitive — toggles ActiveAccount without touching match history. Original
    /// implementation: PussyCatsApp UserProfileRepository.UpdateAccountStatus, which mapped a
    /// "ACTIVE" string to a boolean; that string indirection is dropped, callers pass the bool
    /// directly.
    /// </summary>
    public async Task UpdateActiveAccountAsync(int userId, bool isActive, CancellationToken ct = default)
    {
        var user = await db.Users.FindAsync(new object?[] { userId }, ct).ConfigureAwait(false);
        if (user is null)
        {
            return;
        }
        user.ActiveAccount = isActive;
        user.LastUpdated = DateTime.UtcNow;
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Original: PussyCatsApp UserProfileRepository.UpdateProfilePicture. The original handled
    /// null by writing DBNull; here the column is non-nullable string, so callers pass an empty
    /// string when they intend "no picture".
    /// </summary>
    public async Task UpdateProfilePicturePathAsync(int userId, string profilePicturePath, CancellationToken ct = default)
    {
        var user = await db.Users.FindAsync(new object?[] { userId }, ct).ConfigureAwait(false);
        if (user is null)
        {
            return;
        }
        user.ProfilePicturePath = profilePicturePath;
        user.LastUpdated = DateTime.UtcNow;
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Original: PussyCatsApp UserProfileRepository.UpdateProfileLastModified. The original took
    /// a caller-supplied timestamp; the new contract uses server time so the audit trail is
    /// authoritative.
    /// </summary>
    public async Task TouchLastUpdatedAsync(int userId, CancellationToken ct = default)
    {
        var user = await db.Users.FindAsync(new object?[] { userId }, ct).ConfigureAwait(false);
        if (user is null)
        {
            return;
        }
        user.LastUpdated = DateTime.UtcNow;
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
