using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.PersonalityTests;

public class PersonalityTestRepository : IPersonalityTestRepository
{
    private readonly PussyCatsDbContext db;

    public PersonalityTestRepository(PussyCatsDbContext db)
    {
        this.db = db;
    }

    /// <summary>
    /// Includes TraitScores so a single query returns the complete result. Tracked because
    /// PersonalityTestService updates trait scores in place when the user re-takes the test.
    /// Note: this replaces PussyCatsApp's PersonalityTestRepository.Load, which previously read
    /// a single string column off the Users table and returned that. The new model stores trait
    /// scores as first-class rows, so the return shape is the structured PersonalityTestResult.
    /// </summary>
    public async Task<PersonalityTestResult?> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await db.PersonalityTestResults
            .Include(r => r.TraitScores)
            .FirstOrDefaultAsync(r => r.UserId == userId, ct)
            .ConfigureAwait(false);
    }

    public async Task<PersonalityTestResult> AddAsync(PersonalityTestResult result, CancellationToken ct = default)
    {
        if (result.CompletedAt == default)
        {
            result.CompletedAt = DateTime.UtcNow;
        }
        db.PersonalityTestResults.Add(result);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
        return result;
    }

    public async Task UpdateAsync(PersonalityTestResult result, CancellationToken ct = default)
    {
        db.PersonalityTestResults.Update(result);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int personalityTestResultId, CancellationToken ct = default)
    {
        var result = await db.PersonalityTestResults.FindAsync(new object?[] { personalityTestResultId }, ct).ConfigureAwait(false);
        if (result is null)
        {
            return;
        }
        db.PersonalityTestResults.Remove(result);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
