using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;

namespace PussyCats.App.Services;

/// <summary>Computes role compatibility scores and skill gap suggestions for a candidate.</summary>
public interface ICompatibilityService
{
    Task<RoleResult> CalculateForRoleAsync(int userId, JobRole role, CancellationToken ct = default);

    Task<IReadOnlyList<RoleResult>> CalculateAllAsync(int userId, CancellationToken ct = default);

    IReadOnlyList<Suggestion> GetSuggestions(RoleResult result);
}
