using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

namespace PussyCats.Tests.Services;

public class SkillTestServiceTests
{
    private readonly FakeSkillTestRepository skillTestRepository = new();
    private readonly SkillTestService skillTestService;

    private int ExcellentRetakeScore = 95;
    private const int FailingRetakeScore = 50;

    public SkillTestServiceTests()
    {
        skillTestService = new SkillTestService(skillTestRepository);
    }


    [Fact]
    public async Task CanRetakeTestAsync_TestOlderThanEligibilityWindow_ReturnsTrue()
    {
        DateOnly fourMonthsAgo = DateOnly.FromDateTime(DateTime.Now.AddMonths(-4));
        int skillTestId = 1;

        skillTestRepository.Seed(new SkillTestBuilder()
            .WithId(skillTestId)
            .WithAchievedDate(fourMonthsAgo)
            .Build());

        (await skillTestService.CanRetakeTestAsync(skillTestId)).Should().BeTrue();
    }

    [Fact]
    public async Task CanRetakeTestAsync_TestInsideEligibilityWindow_ReturnsFalse()
    {
        DateOnly oneMonthAgo = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1));
        int skillTestId = 1;

        skillTestRepository.Seed(new SkillTestBuilder()
            .WithId(skillTestId)
            .WithAchievedDate(oneMonthAgo)
            .Build());

        (await skillTestService.CanRetakeTestAsync(skillTestId)).Should().BeFalse();
    }

    [Fact]
    public async Task CanRetakeTestAsync_TestIsMissing_ThrowsException()
    {
        int missingTestId = 100;
        Func<Task> act = () => skillTestService.CanRetakeTestAsync(missingTestId);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*No test found*");
    }

    [Fact]
    public async Task SubmitRetakeAsync_UserIsEligible_UpdatesScoreAndDateAndReturnsBadge()
    {
        int skillTetsId = 1;
        int initialScore = 40;
        DateOnly sixMonthsAgo = DateOnly.FromDateTime(DateTime.Now.AddMonths(-6));
        skillTestRepository.Seed(new SkillTestBuilder()
            .WithId(skillTetsId)
            .WithScore(initialScore)
            .WithAchievedDate(sixMonthsAgo)
            .Build());

        var badge = await skillTestService.SubmitRetakeAsync(skillTetsId, ExcellentRetakeScore);

        var test = await skillTestRepository.GetByIdAsync(skillTetsId);
        test!.Score.Should().Be(ExcellentRetakeScore);
        test.AchievedDate.Should().Be(DateOnly.FromDateTime(DateTime.Now));
        badge.Tier.Should().Be(BadgeTier.Gold);
        badge.ExperiencePointsValue.Should().Be(SimpleModelOperations.GoldExperiencePoints);
    }

    [Fact]
    public async Task SubmitRetakeAsync_UserIsNotYetEligible_ThrowsException()
    {
        int skillTestId = 1;
        DateOnly tenDaysAgo = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
        skillTestRepository.Seed(new SkillTestBuilder()
            .WithId(skillTestId)
            .WithAchievedDate(tenDaysAgo)
            .Build());

        Func<Task> act = () => skillTestService.SubmitRetakeAsync(skillTestId, ExcellentRetakeScore);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*not yet eligible*");
    }


    [Fact]
    public void IsRetakeEligible_TestDatesProvided_EnforcesThreeMonthWindow()
    {
        DateOnly fourMonthsAgo = DateOnly.FromDateTime(DateTime.Now.AddMonths(-4));
        DateOnly thirtyDaysAgo = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
        var oldEnough = new SkillTestBuilder()
            .WithAchievedDate(fourMonthsAgo)
            .Build();
        var tooRecent = new SkillTestBuilder()
            .WithAchievedDate(thirtyDaysAgo)
            .Build();

        SkillTestService.IsRetakeEligible(oldEnough).Should().BeTrue();
        SkillTestService.IsRetakeEligible(tooRecent).Should().BeFalse();
    }
    [Fact]
    public void AchievedDateFormatted_ReturnsFormattedDate()
    {
        string expectedFormattedDate = "12.05.2025";

        DateOnly achievedDate = new(2025, 5, 12);
        var skillTest = new SkillTestBuilder()
            .WithAchievedDate(achievedDate)
            .Build();

        var result = SkillTestService.AchievedDateFormatted(skillTest);

        result.Should().Be(expectedFormattedDate);
    }
}