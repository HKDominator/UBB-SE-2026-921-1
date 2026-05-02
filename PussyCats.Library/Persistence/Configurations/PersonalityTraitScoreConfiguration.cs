using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class PersonalityTraitScoreConfiguration : IEntityTypeConfiguration<PersonalityTraitScore>
{
    public void Configure(EntityTypeBuilder<PersonalityTraitScore> builder)
    {
        builder.ToTable("PersonalityTraitScores");
        builder.HasKey(s => s.PersonalityTraitScoreId);

        builder.Property(s => s.Trait).HasConversion<string>().HasMaxLength(40);

        // Cascade is configured from PersonalityTestResultConfiguration so the trait score rows
        // disappear with their parent.
        builder.HasIndex(s => s.PersonalityTestResultId);
    }
}
