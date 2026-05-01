using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.Recommendations;

public interface IRecommendationRepository
{
    Task<Recommendation?> GetByIdAsync(int recommendationId, CancellationToken ct = default);

    Task<IReadOnlyList<Recommendation>> GetAllAsync(CancellationToken ct = default);

    Task<Recommendation?> GetLatestByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken ct = default);

    Task<Recommendation> AddAsync(Recommendation recommendation, CancellationToken ct = default);

    Task RemoveAsync(int recommendationId, CancellationToken ct = default);
}
