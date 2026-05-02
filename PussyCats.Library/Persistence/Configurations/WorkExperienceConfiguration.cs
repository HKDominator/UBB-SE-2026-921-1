using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class WorkExperienceConfiguration : IEntityTypeConfiguration<WorkExperience>
{
    public void Configure(EntityTypeBuilder<WorkExperience> builder)
    {
        builder.ToTable("WorkExperiences");
        builder.HasKey(w => w.WorkExperienceId);

        builder.Property(w => w.Company).HasMaxLength(200);
        builder.Property(w => w.JobTitle).HasMaxLength(200);
        builder.Property(w => w.Description).HasMaxLength(2000);

        // Cascade is configured on UserConfiguration (User -> WorkExperiences). Index supports
        // the GetByUserId-style queries used during profile loads.
        builder.HasIndex(w => w.UserId);
    }
}
