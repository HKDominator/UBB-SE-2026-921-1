using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.Domain;

public class PersonalityTraitScore
{
    public int PersonalityTraitScoreId { get; set; }

    public int PersonalityTestResultId { get; set; }
    public PersonalityTestResult PersonalityTestResult { get; set; } = null!;

    public TraitType Trait { get; set; }
    public int Score { get; set; }
}
