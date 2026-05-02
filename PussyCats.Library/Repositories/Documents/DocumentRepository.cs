using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Documents;

public class DocumentRepository : IDocumentRepository
{
    private readonly PussyCatsDbContext db;

    public DocumentRepository(PussyCatsDbContext db)
    {
        this.db = db;
    }

    /// <summary>
    /// Tracked — typical caller (FilesController.Delete) mutates immediately. No User include
    /// because the path is already enough to serve the file.
    /// </summary>
    public async Task<Document?> GetByIdAsync(int documentId, CancellationToken ct = default)
    {
        return await db.Documents
            .FirstOrDefaultAsync(d => d.DocumentId == documentId, ct)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Original: PussyCatsApp DocumentRepository.GetDocumentsByUserId — straight predicate port.
    /// Read-only listing.
    /// </summary>
    public async Task<IReadOnlyList<Document>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await db.Documents
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<Document> AddAsync(Document document, CancellationToken ct = default)
    {
        if (document.UploadDate == default)
        {
            document.UploadDate = DateTime.UtcNow;
        }
        db.Documents.Add(document);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
        return document;
    }

    public async Task RemoveAsync(int documentId, CancellationToken ct = default)
    {
        var document = await db.Documents.FindAsync(new object?[] { documentId }, ct).ConfigureAwait(false);
        if (document is null)
        {
            return;
        }
        db.Documents.Remove(document);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
