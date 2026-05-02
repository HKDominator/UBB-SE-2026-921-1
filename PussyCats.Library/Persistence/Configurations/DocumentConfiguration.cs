using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");
        builder.HasKey(d => d.DocumentId);

        builder.Property(d => d.DocumentName).HasMaxLength(200);
        // FilePath stores the path on the API server's filesystem (per MergePlan §4 — the binary
        // never lives in the database).
        builder.Property(d => d.FilePath).HasMaxLength(512);

        // Cascade: deleting a user wipes their owned document records. The actual files on disk
        // need to be cleaned up by the API service layer; the database only tracks metadata.
        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.UserId);
    }
}
