using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.PersonalityTests;

public class QuestionRepository : IQuestionRepository
{
    private readonly PussyCatsDbContext db;

    public QuestionRepository(PussyCatsDbContext db)
    {
        this.db = db;
    }

    public async Task<Question?> GetByIdAsync(int questionId, CancellationToken ct = default)
    {
        return await db.Questions
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.QuestionId == questionId, ct)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the catalog ordered by SortOrder so the UI can render questions in the original
    /// designer-defined sequence. Read-only.
    /// </summary>
    public async Task<IReadOnlyList<Question>> GetAllOrderedAsync(CancellationToken ct = default)
    {
        return await db.Questions
            .AsNoTracking()
            .OrderBy(q => q.SortOrder)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }
}
