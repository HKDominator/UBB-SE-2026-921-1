using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class RecommendationConfiguration : IEntityTypeConfiguration<Recommendation>
{
    public void Configure(EntityTypeBuilder<Recommendation> builder)
    {
        builder.ToTable("Recommendations");
        builder.HasKey(r => r.RecommendationId);

        // Cascade: User -> Recommendation. Per-user recommendations are derived from the user's
        // own profile; once the user is gone there is nothing meaningful to keep.
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict on Job -> Recommendation. SQL Server forbids two cascade paths converging on
        // one table, and User -> Recommendation already cascades; restricting here also guards
        // against accidentally deleting recommendations across all users when a job is removed.
        builder.HasOne(r => r.Job)
            .WithMany()
            .HasForeignKey(r => r.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.JobId);
        // (UserId, JobId, Timestamp DESC) supports GetLatestByUserIdAndJobIdAsync without a sort.
        builder.HasIndex(r => new { r.UserId, r.JobId, r.Timestamp });
    }
}
