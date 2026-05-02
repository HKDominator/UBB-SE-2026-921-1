using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Recommendations;

public class RecommendationRepository : IRecommendationRepository
{
    private readonly PussyCatsDbContext db;

    public RecommendationRepository(PussyCatsDbContext db)
    {
        this.db = db;
    }

    /// <summary>
    /// Tracked single-row lookup. No User/Job include — callers that need them ask for the User
    /// or Job repository directly.
    /// </summary>
    public async Task<Recommendation?> GetByIdAsync(int recommendationId, CancellationToken ct = default)
    {
        return await db.Recommendations
            .FirstOrDefaultAsync(r => r.RecommendationId == recommendationId, ct)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Recommendation>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Recommendations
            .AsNoTracking()
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Original: matchmaking SqlRecommendationRepository.GetLatestByUserIdAndJobId — preserves
    /// the WHERE+ORDER BY Timestamp DESC TOP(1) semantics by ordering on Timestamp descending
    /// and taking the first row.
    /// </summary>
    public async Task<Recommendation?> GetLatestByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken ct = default)
    {
        return await db.Recommendations
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.JobId == jobId)
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<Recommendation> AddAsync(Recommendation recommendation, CancellationToken ct = default)
    {
        if (recommendation.Timestamp == default)
        {
            recommendation.Timestamp = DateTime.UtcNow;
        }
        db.Recommendations.Add(recommendation);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
        return recommendation;
    }

    public async Task RemoveAsync(int recommendationId, CancellationToken ct = default)
    {
        var recommendation = await db.Recommendations.FindAsync(new object?[] { recommendationId }, ct).ConfigureAwait(false);
        if (recommendation is null)
        {
            return;
        }
        db.Recommendations.Remove(recommendation);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
