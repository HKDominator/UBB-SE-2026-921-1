using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Helpers;
using PussyCats.Library.Repositories.Documents;
using PussyCats.Library.Services.FileStorage;

namespace PussyCats.Library.Services.Documents;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository documentRepository;
    private readonly ILocalFileStorageService fileStorage;

    public DocumentService(IDocumentRepository documentRepository, ILocalFileStorageService fileStorage)
    {
        this.documentRepository = documentRepository;
        this.fileStorage = fileStorage;
    }

    public async Task<IReadOnlyList<Document>> GetDocumentsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await documentRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Document> UploadDocumentAsync(Document document, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        if (!ValidateFileType(extension))
            throw new InvalidOperationException("Invalid file type. Only PDF, JPG, and PNG files are accepted.");

        var storedPath = await fileStorage.SaveFileAsync(fileStream, fileName, cancellationToken).ConfigureAwait(false);

        document.FilePath = storedPath;
        document.UploadDate = DateTime.Now;

        return await documentRepository.AddAsync(document, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteDocumentAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var document = await documentRepository.GetByIdAsync(documentId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Document not found.");

        if (!string.IsNullOrEmpty(document.FilePath))
            await fileStorage.DeleteFileAsync(document.FilePath, cancellationToken).ConfigureAwait(false);

        await documentRepository.RemoveAsync(documentId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetDocumentPathAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var document = await documentRepository.GetByIdAsync(documentId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Document not found.");

        return fileStorage.GetUrl(document.FilePath);
    }

    private static bool ValidateFileType(string extension)
    {
        var normalised = extension.TrimStart('.');
        return Enum.TryParse<AllowedFileType>(normalised, ignoreCase: true, out _);
    }
}