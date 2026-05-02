using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");
        builder.HasKey(q => q.QuestionId);

        builder.Property(q => q.QuestionText).HasMaxLength(1000).IsRequired();
        builder.Property(q => q.Trait).HasConversion<string>().HasMaxLength(40);

        // SortOrder drives the GetAllOrderedAsync query — index makes the ORDER BY scan-free.
        builder.HasIndex(q => q.SortOrder);

        // Personality test questions stay in code (per the user spec); no HasData seeding here.
    }
}
