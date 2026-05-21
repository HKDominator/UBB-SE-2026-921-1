using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.Documents;

namespace PussyCats.Web.ServiceProxies;

public class DocumentServiceProxy : IDocumentService
{
    private readonly HttpClient http;

    public DocumentServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<IReadOnlyList<Document>> GetDocumentsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<Document>>($"api/documents?userId={userId}", cancellationToken) ?? new List<Document>();


    public async Task<Document> UploadDocumentAsync(Document document, string filePath, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("File upload is not supported via the HTTP proxy.");

    public async Task DeleteDocumentAsync(int documentId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("File deletion is not supported via the HTTP proxy.");

    public async Task<string> GetDocumentPathAsync(int documentId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Path retrieval is not supported via the HTTP proxy.");
}

