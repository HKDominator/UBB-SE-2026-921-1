using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.ChatService;

namespace PussyCats.App.ServiceProxies;

public class ChatServiceProxy : IChatService
{
    private readonly HttpClient http;

    public ChatServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<Chat?> FindOrCreateUserCompanyChatAsync(int userId, Company company, Job? job = null, CancellationToken cancellationToken = default)
    {
        var url = $"api/chat/user-company?userId={userId}{(job is not null ? $"&jobId={job.JobId}" : "")}";
        var response = await http.PostAsJsonAsync(url, company, JsonOptions.Default, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Chat>(JsonOptions.Default, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Chat?> FindOrCreateUserChatAsync(int userId, int secondUserId, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync($"api/chat/user-user?userId={userId}&secondUserId={secondUserId}", (object?)null, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Chat>(JsonOptions.Default, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Chat>> GetChatsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await http.GetFromJsonAsync<IReadOnlyList<Chat>>($"api/chat/user/{userId}", JsonOptions.Default, cancellationToken).ConfigureAwait(false)
               ?? [];
    }

    public async Task<IReadOnlyList<Chat>> GetChatsForCompanyAsync(int companyId, CancellationToken cancellationToken = default)
    {
        return await http.GetFromJsonAsync<IReadOnlyList<Chat>>($"api/chat/company/{companyId}", JsonOptions.Default, cancellationToken).ConfigureAwait(false)
               ?? [];
    }

    public async Task<IReadOnlyList<Message>> GetMessagesAsync(int chatId, int callerId, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/chat/{chatId}/messages?callerId={callerId}", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<Message>>(JsonOptions.Default, cancellationToken).ConfigureAwait(false)
               ?? [];
    }

    public async Task<IReadOnlyList<Company>> SearchCompaniesAsync(string companyNameSearchTerm, CancellationToken cancellationToken = default)
    {
        return await http.GetFromJsonAsync<IReadOnlyList<Company>>($"api/chat/search/companies?term={Uri.EscapeDataString(companyNameSearchTerm)}", JsonOptions.Default, cancellationToken).ConfigureAwait(false)
               ?? [];
    }

    public async Task<IReadOnlyList<User>> SearchUsersAsync(string userNameSearchTerm, CancellationToken cancellationToken = default)
    {
        return await http.GetFromJsonAsync<IReadOnlyList<User>>($"api/chat/search/users?term={Uri.EscapeDataString(userNameSearchTerm)}", JsonOptions.Default, cancellationToken).ConfigureAwait(false)
               ?? [];
    }

    public async Task SendMessageAsync(int chatId, string content, int senderId, MessageType typeOfMessage, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync($"api/chat/{chatId}/messages?senderId={senderId}&type={typeOfMessage}", content, JsonOptions.Default, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Stream> OpenMessageAttachmentAsync(string attachmentPath, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/chat/messages/attachment?attachmentPath={Uri.EscapeDataString(attachmentPath)}", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var memoryStream = new MemoryStream();
        await response.Content.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task MarkMessagesAsReadAsync(int chatId, int readerId, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsync($"api/chat/{chatId}/messages/read?readerId={readerId}", null, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task BlockChatAsync(int chatId, int blockerId, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsync($"api/chat/{chatId}/block?blockerId={blockerId}", null, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task UnblockChatAsync(int chatId, int unblockerId, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsync($"api/chat/{chatId}/unblock?unblockerId={unblockerId}", null, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteChatAsync(int chatId, int callerId, CancellationToken cancellationToken = default)
    {
        var response = await http.DeleteAsync($"api/chat/{chatId}?callerId={callerId}", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }
}