using PussyCats.Library.Domain;

namespace PussyCats.Library.Services.Documents;

public interface IDocumentService
{
    Task<IReadOnlyList<Document>> GetDocumentsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Document> UploadDocumentAsync(Document document, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task DeleteDocumentAsync(int documentId, CancellationToken cancellationToken = default);
    Task<string> GetDocumentPathAsync(int documentId, CancellationToken cancellationToken = default);
}