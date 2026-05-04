using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.Domain;

public class PersonalityTestResult
{
    public int PersonalityTestResultId { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CompletedAt { get; set; }

    public JobRole? SelectedRole { get; set; }

    public List<PersonalityTraitScore> TraitScores { get; set; } = new();
}
