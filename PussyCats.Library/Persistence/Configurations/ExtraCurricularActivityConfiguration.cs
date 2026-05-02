using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class ExtraCurricularActivityConfiguration : IEntityTypeConfiguration<ExtraCurricularActivity>
{
    public void Configure(EntityTypeBuilder<ExtraCurricularActivity> builder)
    {
        builder.ToTable("ExtraCurricularActivities");
        builder.HasKey(a => a.ExtraCurricularActivityId);

        builder.Property(a => a.ActivityName).HasMaxLength(200);
        builder.Property(a => a.Organization).HasMaxLength(200);
        builder.Property(a => a.Role).HasMaxLength(200);
        builder.Property(a => a.Period).HasMaxLength(100);
        builder.Property(a => a.Description).HasMaxLength(2000);

        // Cascade configured on UserConfiguration (User -> ExtraCurricularActivities).
        builder.HasIndex(a => a.UserId);
    }
}
