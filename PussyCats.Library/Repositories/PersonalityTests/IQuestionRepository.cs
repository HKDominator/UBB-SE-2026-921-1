using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.PersonalityTests;

public interface IQuestionRepository
{
    Task<Question?> GetByIdAsync(int questionId, CancellationToken ct = default);

    Task<IReadOnlyList<Question>> GetAllOrderedAsync(CancellationToken ct = default);
}
