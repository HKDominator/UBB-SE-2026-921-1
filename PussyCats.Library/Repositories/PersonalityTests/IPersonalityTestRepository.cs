using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.PersonalityTests;

public interface IPersonalityTestRepository
{
    Task<PersonalityTestResult?> GetByUserIdAsync(int userId, CancellationToken ct = default);

    Task<PersonalityTestResult> AddAsync(PersonalityTestResult result, CancellationToken ct = default);

    Task UpdateAsync(PersonalityTestResult result, CancellationToken ct = default);

    Task RemoveAsync(int personalityTestResultId, CancellationToken ct = default);
}
