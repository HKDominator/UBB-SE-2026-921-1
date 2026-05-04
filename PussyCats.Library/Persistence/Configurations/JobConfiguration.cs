using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.Persistence.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs");
        builder.HasKey(j => j.JobId);

        builder.Property(j => j.JobTitle).HasMaxLength(200).IsRequired();
        builder.Property(j => j.JobDescription).HasMaxLength(4000);
        builder.Property(j => j.Location).HasMaxLength(100);
        builder.Property(j => j.EmploymentType).HasMaxLength(40);

        // Stored as int by EF convention; no HasConversion needed.
        builder.Property(j => j.JobRole);

        // Restrict: deleting a company should not nuke its job postings. Archive instead.
        builder.HasOne(j => j.Company)
            .WithMany(c => c.Jobs)
            .HasForeignKey(j => j.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // JobSkill cascade: when a job is removed its skill requirements go too (configured on
        // JobSkillConfiguration). Match relationships are restricted (configured on
        // MatchConfiguration) so historical applications survive job archival.

        // Indexes for common API filters: GET /api/jobs?location=&type=
        builder.HasIndex(j => j.Location);
        builder.HasIndex(j => j.EmploymentType);
        // CompanyId is heavily filtered on (GetByCompanyIdAsync) — explicit non-unique index.
        builder.HasIndex(j => j.CompanyId);

        builder.HasData(
            new Job
            {
                JobId = 1,
                JobTitle = "Backend .NET Developer",
                JobDescription = "Join our Bucharest team building enterprise APIs and integrations. Strong C# and SQL; experience with Azure or containers is a plus.",
                Location = "Bucharest",
                EmploymentType = "Hybrid",
                CompanyId = 1,
                PromotionLevel = 2,
                JobRole = JobRole.BackendDeveloper,
            },
            new Job
            {
                JobId = 2,
                JobTitle = "Junior Frontend Developer",
                JobDescription = "Ship UI features for our web app under mentorship. Learn React, testing, and our design system while pairing with senior engineers.",
                Location = "Cluj-Napoca",
                EmploymentType = "Full-time",
                CompanyId = 2,
                PromotionLevel = 1,
                JobRole = JobRole.FrontendDeveloper,
            },
            new Job
            {
                JobId = 3,
                JobTitle = "Data Analyst",
                JobDescription = "Turn business questions into dashboards and ad hoc analyses. SQL and visualization tools; curiosity about the domain.",
                Location = "Brasov",
                EmploymentType = "Hybrid",
                CompanyId = 3,
                PromotionLevel = 1,
                JobRole = JobRole.DataAnalyst,
            });
    }
}
