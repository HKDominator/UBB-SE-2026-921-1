using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class PersonalityTestResultConfiguration : IEntityTypeConfiguration<PersonalityTestResult>
{
    public void Configure(EntityTypeBuilder<PersonalityTestResult> builder)
    {
        builder.ToTable("PersonalityTestResults");
        builder.HasKey(r => r.PersonalityTestResultId);

        // Cascade: deleting the result removes its trait scores. The User -> PersonalityResult
        // cascade is configured on UserConfiguration.
        builder.HasMany(r => r.TraitScores)
            .WithOne(s => s.PersonalityTestResult)
            .HasForeignKey(s => s.PersonalityTestResultId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserId is the natural lookup column (GetByUserIdAsync). The User -> PersonalityResult
        // relationship is one-to-zero-or-one, so the FK is unique.
        builder.HasIndex(r => r.UserId).IsUnique();
    }
}
