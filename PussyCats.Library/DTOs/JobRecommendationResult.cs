using PussyCats.Library.Domain;

namespace PussyCats.Library.DTOs;

public sealed class JobRecommendationResult
{
    public required Job Job { get; init; }
    public required Company Company { get; init; }
    public double CompatibilityScore { get; init; }
    public int? DisplayRecommendationId { get; init; }
    public IReadOnlyList<string> TopSkillLabels { get; init; } = new List<string>();
    public IReadOnlyList<string> AllSkillLabels { get; init; } = new List<string>();
}
