using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

namespace PussyCats.Tests.Services;

public class CompanyStatusServiceTests
{
    private readonly FakeMatchRepository matchRepo = new();
    private readonly FakeJobRepository jobRepo = new();
    private readonly FakeUserRepository userRepo = new();
    private readonly FakeUserSkillRepository userSkillRepo = new();
    private readonly CompanyStatusService service;

    public CompanyStatusServiceTests()
    {
        var jobService = new JobService(jobRepo);
        service = new CompanyStatusService(
            new MatchService(matchRepo, jobService, new UserService(userRepo)),
            new UserService(userRepo),
            jobService,
            new UserSkillService(userSkillRepo));
    }

    [Fact]
    public async Task GetApplicantsForCompanyAsync_MatchesAreNotYetDecided_ReturnsOnlyDecidedMatches()
    {
        const int firstUserId = 1, secondUserId = 2;
        const int jobId = 10, companyId = 5;
        userRepo.Seed(new UserBuilder().WithId(firstUserId).Build(), new UserBuilder().WithId(secondUserId).Build());
        jobRepo.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        matchRepo.Seed(
            new MatchBuilder().WithId(1).AppliedFor(firstUserId, jobId).WithStatus(MatchStatus.Applied).Build(),
            new MatchBuilder().WithId(2).AppliedFor(secondUserId, jobId).WithStatus(MatchStatus.Accepted).Build());

        var result = await service.GetApplicantsForCompanyAsync(companyId);

        const int expectedNumberOfApplicants = 1, expectedApplicantId = 2;

        result.Should().HaveCount(expectedNumberOfApplicants);
        result[0].Match.MatchId.Should().Be(expectedApplicantId);
    }

    [Fact]
    public async Task GetApplicantsForCompanyAsync_MatchesAreAdvancedOrRejected_IncludesAdvancedAndRejectedMatches()
    {
        const int firstUserId = 1, secondUserId = 2;
        const int jobId = 10, companyId = 5;
        userRepo.Seed(new UserBuilder().WithId(firstUserId).Build(), new UserBuilder().WithId(secondUserId).Build());
        jobRepo.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        matchRepo.Seed(
            new MatchBuilder().WithId(1).AppliedFor(firstUserId, jobId).WithStatus(MatchStatus.Advanced).Build(),
            new MatchBuilder().WithId(2).AppliedFor(secondUserId, jobId).WithStatus(MatchStatus.Rejected).Build());

        var result = await service.GetApplicantsForCompanyAsync(companyId);

        int expectedNumberOfApplicants = 2;
        result.Should().HaveCount(expectedNumberOfApplicants);
    }

    [Fact]
    public async Task GetApplicantsForCompanyAsync_UserOrJobIsMissing_SkipsMatchesWithMissingData()
    {
        const int jobId = 10, companyId = 5;
        const int nonExistentUserId = 99;
        jobRepo.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        matchRepo.Seed(new MatchBuilder()
            .WithId(1)
            .AppliedFor(nonExistentUserId, jobId)
            .WithStatus(MatchStatus.Accepted)
            .Build());

        var result = await service.GetApplicantsForCompanyAsync(companyId);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetApplicantsForCompanyAsync_MultipleApplicantsExist_SortsDescendingByCompatibilityScore()
    {
        const int firstUserId = 1, secondUserId = 2;
        const string firstUserCity = "Bucharest", secondUserCity = "Bucharest, Romania";
        const int jobId = 10, companyId = 5;
        const int skillId = 1, firstUserScore = 60, secondUserScore = 90;

        userRepo.Seed(
            new UserBuilder().WithId(firstUserId).WithCity(firstUserCity).Build(),
            new UserBuilder().WithId(secondUserId).WithCity(secondUserCity).Build());
        jobRepo.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).WithLocation("Bucharest, Romania").Build());
        userSkillRepo.Seed(
            new UserSkill { User = new User { UserId = firstUserId }, Skill = new Skill { SkillId = skillId }, Score = firstUserScore },
            new UserSkill { User = new User { UserId = secondUserId }, Skill = new Skill { SkillId = skillId }, Score = secondUserScore });
        matchRepo.Seed(
            new MatchBuilder().WithId(1).AppliedFor(firstUserId, jobId).WithStatus(MatchStatus.Accepted).Build(),
            new MatchBuilder().WithId(2).AppliedFor(secondUserId, jobId).WithStatus(MatchStatus.Accepted).Build());

        var result = await service.GetApplicantsForCompanyAsync(companyId);

        const int expectedNumberOfApplicants = 2;
        result.Should().HaveCount(expectedNumberOfApplicants);
        result[0].CompatibilityScore.Should().BeGreaterThan(result[1].CompatibilityScore);
    }

    [Fact]
    public async Task GetApplicantsForCompanyAsync_JobLocationIncludesCountry_AppliesLocationBonus()
    {
        const int userId = 1, jobId = 10, companyId = 5, skillId = 1, userSkillScore = 50;
        const string city = "Bucharest", jobLocation = "Bucharest, Romania";
        userRepo.Seed(new UserBuilder().WithId(userId).WithCity(city).Build());
        jobRepo.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).WithLocation(jobLocation).Build());

        userSkillRepo.Seed(new UserSkill { User = new User { UserId = userId }, Skill = new Skill { SkillId = skillId }, Score = userSkillScore });

        matchRepo.Seed(new MatchBuilder().WithId(1).AppliedFor(userId, jobId).WithStatus(MatchStatus.Accepted).Build());

        var result = await service.GetApplicantsForCompanyAsync(companyId);

        const int expectedCompatibilityScore = 60; // 50 base score + 10 location bonus

        result[0].CompatibilityScore.Should().Be(expectedCompatibilityScore);
    }

    [Fact]
    public async Task GetApplicantByMatchIdAsync_MatchExists_ReturnsSpecificApplicant()
    {
        const int userId = 1, jobId = 10, companyId = 5;
        const int matchId = 1;
        userRepo.Seed(new UserBuilder().WithId(userId).Build());
        jobRepo.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        matchRepo.Seed(new MatchBuilder().WithId(matchId).AppliedFor(userId, jobId).WithStatus(MatchStatus.Accepted).Build());

        var result = await service.GetApplicantByMatchIdAsync(companyId, matchId);

        result.Should().NotBeNull();
        result!.Match.MatchId.Should().Be(matchId);
    }

    [Fact]
    public async Task GetApplicantByMatchIdAsync_MatchIsMissing_ReturnsNull()
    {
        const int nonExistentCompanyId = 5, nonExistentMatchId = 999;
        var result = await service.GetApplicantByMatchIdAsync(nonExistentCompanyId, nonExistentMatchId);

        result.Should().BeNull();
    }
}
