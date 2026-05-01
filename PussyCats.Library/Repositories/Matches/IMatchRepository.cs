using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.Matches;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(int matchId, CancellationToken ct = default);

    Task<IReadOnlyList<Match>> GetAllAsync(CancellationToken ct = default);

    Task<IReadOnlyList<Match>> GetByUserIdAsync(int userId, CancellationToken ct = default);

    Task<Match?> GetByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken ct = default);

    Task<Match> AddAsync(Match match, CancellationToken ct = default);

    Task UpdateAsync(Match match, CancellationToken ct = default);

    Task RemoveAsync(int matchId, CancellationToken ct = default);
}
