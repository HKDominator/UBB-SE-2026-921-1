using System.Net.Http.Headers;
using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Helpers;
using PussyCats.Library.Services.Documents;

namespace PussyCats.App.ServiceProxies;

public class DocumentServiceProxy : IDocumentService
{
    private readonly HttpClient http;

    public DocumentServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<IReadOnlyList<Document>> GetDocumentsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await http.GetFromJsonAsync<IReadOnlyList<Document>>($"api/documents/user/{userId}", JsonOptions.Default, cancellationToken).ConfigureAwait(false)
               ?? [];
    }

    public async Task<Document> UploadDocumentAsync(Document document, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        using var multipart = new MultipartFormDataContent();
        multipart.Add(new StringContent(document.User.UserId.ToString()), "userId");
        multipart.Add(new StringContent(document.DocumentName), "documentName");

        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        multipart.Add(streamContent, "file", fileName);

        var response = await http.PostAsync("api/documents/upload", multipart, cancellationToken).ConfigureAwait(false);

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Upload failed ({response.StatusCode}): {body}");

        return System.Text.Json.JsonSerializer.Deserialize<Document>(body, JsonOptions.Default)
               ?? throw new InvalidOperationException("No document returned after upload.");
    }

    public async Task DeleteDocumentAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var response = await http.DeleteAsync($"api/documents/{documentId}", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string> GetDocumentPathAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/documents/{documentId}/path", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        string relativePathOfDocumentOnServer= await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        string uriPath= http.BaseAddress + "api/files/"+ relativePathOfDocumentOnServer;
        return uriPath;
    }
}