using System.Text;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.SkillTests;
using PussyCats.Library.Repositories.Users;

namespace PussyCats.App.Services;

public class UserProfileService : IUserProfileService
{
    // XP thresholds — SkillTestService.GetExperiencePoints (3b.2) uses the same values.
    private const int GoldScoreThreshold = 90;
    private const int SilverScoreThreshold = 70;
    private const int BronzeScoreThreshold = 50;
    private const int GoldExperiencePoints = 100;
    private const int SilverExperiencePoints = 60;
    private const int BronzeExperiencePoints = 30;
    private const int ParticipantExperiencePoints = 10;

    // Level thresholds — UserLevelService.CalculateLevel (3b.2) uses the same values.
    private const int Level2ExperiencePoints = 100;
    private const int Level3ExperiencePoints = 250;
    private const int Level4ExperiencePoints = 500;
    private const int Level5ExperiencePoints = 800;

    private readonly IUserRepository userRepository;
    private readonly ISkillTestRepository skillTestRepository;

    public UserProfileService(IUserRepository userRepository, ISkillTestRepository skillTestRepository)
    {
        this.userRepository = userRepository;
        this.skillTestRepository = skillTestRepository;
    }

    public async Task<User?> GetProfileAsync(int userId, CancellationToken ct = default)
    {
        return await userRepository.GetByIdAsync(userId, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<SkillTest>> GetSkillTestsForUserAsync(int userId, CancellationToken ct = default)
    {
        return await skillTestRepository.GetByUserIdAsync(userId, ct).ConfigureAwait(false);
    }

    public async Task<bool> IsProfileAvailableAsync(int userId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct).ConfigureAwait(false);

        if (user is null)
        {
            throw new Exception($"No profile found for ID {userId}");
        }

        return user.ActiveAccount;
    }

    public async Task UpdateAccountStatusAsync(int userId, bool isActive, CancellationToken ct = default)
    {
        await userRepository.UpdateActiveAccountAsync(userId, isActive, ct).ConfigureAwait(false);
        await userRepository.TouchLastUpdatedAsync(userId, ct).ConfigureAwait(false);
    }

    public async Task UpdateProfilePicturePathAsync(int userId, string newPath, CancellationToken ct = default)
    {
        await userRepository.UpdateProfilePicturePathAsync(userId, newPath ?? string.Empty, ct).ConfigureAwait(false);
        await userRepository.TouchLastUpdatedAsync(userId, ct).ConfigureAwait(false);
    }

    public async Task RemoveProfilePicturePathAsync(int userId, CancellationToken ct = default)
    {
        await UpdateProfilePicturePathAsync(userId, string.Empty, ct).ConfigureAwait(false);
    }

    public string GenerateParsedCvText(User user)
    {
        if (user is null)
        {
            return string.Empty;
        }

        var parsedCvTextBuilder = new StringBuilder();
        parsedCvTextBuilder.AppendLine($"{user.FirstName} {user.LastName}".Trim());
        parsedCvTextBuilder.AppendLine(user.University ?? string.Empty);
        parsedCvTextBuilder.AppendLine(string.Join(", ", user.Skills.Select(s => s.Skill?.Name ?? string.Empty).Where(n => !string.IsNullOrEmpty(n))));
        return parsedCvTextBuilder.ToString().TrimEnd();
    }

    public async Task SaveAsync(int userId, User user, CancellationToken ct = default)
    {
        var existing = await userRepository.GetByIdAsync(userId, ct).ConfigureAwait(false);
        if (existing is null)
        {
            await userRepository.AddAsync(user, ct).ConfigureAwait(false);
        }
        else
        {
            await userRepository.UpdateAsync(user, ct).ConfigureAwait(false);
        }
    }

    public async Task<int> RecalculateLevelAsync(User user, CancellationToken ct = default)
    {
        if (user is null)
        {
            return 0;
        }

        var skillTests = await skillTestRepository.GetByUserIdAsync(user.UserId, ct).ConfigureAwait(false);
        int totalExperiencePoints = 0;

        foreach (var skillTest in skillTests)
        {
            totalExperiencePoints += GetExperiencePoints(skillTest);
        }

        user.TotalExperiencePoints = totalExperiencePoints;
        user.CurrentLevel = CalculateLevelNumber(totalExperiencePoints);

        return totalExperiencePoints;
    }

    private static int GetExperiencePoints(SkillTest skillTest)
    {
        if (skillTest.Score >= GoldScoreThreshold)
        {
            return GoldExperiencePoints;
        }

        if (skillTest.Score >= SilverScoreThreshold)
        {
            return SilverExperiencePoints;
        }

        if (skillTest.Score >= BronzeScoreThreshold)
        {
            return BronzeExperiencePoints;
        }

        return ParticipantExperiencePoints;
    }

    private static int CalculateLevelNumber(int experiencePoints)
    {
        if (experiencePoints >= Level5ExperiencePoints)
        {
            return 5;
        }

        if (experiencePoints >= Level4ExperiencePoints)
        {
            return 4;
        }

        if (experiencePoints >= Level3ExperiencePoints)
        {
            return 3;
        }

        if (experiencePoints >= Level2ExperiencePoints)
        {
            return 2;
        }

        return 1;
    }
}
