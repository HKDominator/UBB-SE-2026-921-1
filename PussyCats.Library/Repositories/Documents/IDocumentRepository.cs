using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.Documents;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(int documentId, CancellationToken ct = default);

    Task<IReadOnlyList<Document>> GetByUserIdAsync(int userId, CancellationToken ct = default);

    Task<Document> AddAsync(Document document, CancellationToken ct = default);

    Task RemoveAsync(int documentId, CancellationToken ct = default);
}
