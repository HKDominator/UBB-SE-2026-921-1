using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(c => c.CompanyId);

        builder.Property(c => c.CompanyName).HasMaxLength(200).IsRequired();
        builder.Property(c => c.LogoText).HasMaxLength(20);
        builder.Property(c => c.Email).HasMaxLength(256);
        builder.Property(c => c.Phone).HasMaxLength(40);

        // Company -> Job is configured from the Job side as Restrict — deleting a company should
        // not silently nuke its postings; archive instead.

        builder.HasData(
            new Company { CompanyId = 1, CompanyName = "TechNova", LogoText = "TN", Email = "hr@technova.com", Phone = "0311000001" },
            new Company { CompanyId = 2, CompanyName = "CloudWorks", LogoText = "CW", Email = "jobs@cloudworks.com", Phone = "0311000002" },
            new Company { CompanyId = 3, CompanyName = "DataForge", LogoText = "DF", Email = "careers@dataforge.com", Phone = "0311000003" });
    }
}
