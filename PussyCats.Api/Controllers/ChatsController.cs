using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Helpers;
using PussyCats.Library.Services.ChatService;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService chatService;

    public ChatController(IChatService chatService)
    {
        this.chatService = chatService;
    }

    [HttpPost("user-company")]
    public async Task<IActionResult> FindOrCreateUserCompanyChat(
        [FromQuery] int userId,
        [FromBody] Company company,
        [FromQuery] int? jobId,
        CancellationToken cancellationToken)
    {
        var chat = await chatService.FindOrCreateUserCompanyChatAsync(userId, company, cancellationToken: cancellationToken).ConfigureAwait(false);
        return chat is null ? NotFound() : Ok(chat);
    }

    [HttpPost("user-user")]
    public async Task<IActionResult> FindOrCreateUserChat(
        [FromQuery] int userId,
        [FromQuery] int secondUserId,
        CancellationToken cancellationToken)
    {
        var chat = await chatService.FindOrCreateUserChatAsync(userId, secondUserId, cancellationToken).ConfigureAwait(false);
        return chat is null ? NotFound() : Ok(chat);
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetChatsForUser(int userId, CancellationToken cancellationToken)
    {
        var chats = await chatService.GetChatsForUserAsync(userId, cancellationToken).ConfigureAwait(false);
        return Ok(chats);
    }

    [HttpGet("company/{companyId:int}")]
    public async Task<IActionResult> GetChatsForCompany(int companyId, CancellationToken cancellationToken)
    {
        var chats = await chatService.GetChatsForCompanyAsync(companyId, cancellationToken).ConfigureAwait(false);
        return Ok(chats);
    }

    [HttpGet("{chatId:int}/messages")]
    public async Task<IActionResult> GetMessages(int chatId, [FromQuery] int callerId, CancellationToken cancellationToken)
    {
        try
        {
            var messages = await chatService.GetMessagesAsync(chatId, callerId, cancellationToken).ConfigureAwait(false);
            return Ok(messages);
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
    }

    [HttpGet("search/companies")]
    public async Task<IActionResult> SearchCompanies([FromQuery] string term, CancellationToken cancellationToken)
    {
        var companies = await chatService.SearchCompaniesAsync(term, cancellationToken).ConfigureAwait(false);
        return Ok(companies);
    }

    [HttpGet("search/users")]
    public async Task<IActionResult> SearchUsers([FromQuery] string term, CancellationToken cancellationToken)
    {
        var users = await chatService.SearchUsersAsync(term, cancellationToken).ConfigureAwait(false);
        return Ok(users);
    }

    [HttpPost("{chatId:int}/messages")]
    public async Task<IActionResult> SendMessage(
        int chatId,
        [FromQuery] int senderId,
        [FromQuery] MessageType type,
        [FromBody] string content,
        CancellationToken cancellationToken)
    {
        try
        {
            await chatService.SendMessageAsync(chatId, content, senderId, type, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("messages/attachment")]
    public async Task<IActionResult> GetAttachment([FromQuery] string attachmentPath, CancellationToken cancellationToken)
    {
        try
        {
            var stream = await chatService.OpenMessageAttachmentAsync(attachmentPath, cancellationToken).ConfigureAwait(false);
            DebugToFile.Write("ChatsController","getAttachment "+attachmentPath+" : "+stream);
            var contentType = Path.GetExtension(attachmentPath).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                _ => "application/octet-stream"
            };
            return File(stream, contentType, Path.GetFileName(attachmentPath));
        }
        catch (FileNotFoundException ex) { return NotFound(ex.Message); }
    }

    [HttpPost("{chatId:int}/messages/read")]
    public async Task<IActionResult> MarkMessagesAsRead(int chatId, [FromQuery] int readerId, CancellationToken cancellationToken)
    {
        await chatService.MarkMessagesAsReadAsync(chatId, readerId, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("{chatId:int}/block")]
    public async Task<IActionResult> BlockChat(int chatId, [FromQuery] int blockerId, CancellationToken cancellationToken)
    {
        try
        {
            await chatService.BlockChatAsync(chatId, blockerId, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
    }

    [HttpPost("{chatId:int}/unblock")]
    public async Task<IActionResult> UnblockChat(int chatId, [FromQuery] int unblockerId, CancellationToken cancellationToken)
    {
        try
        {
            await chatService.UnblockChatAsync(chatId, unblockerId, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
    }

    [HttpDelete("{chatId:int}")]
    public async Task<IActionResult> DeleteChat(int chatId, [FromQuery] int callerId, CancellationToken cancellationToken)
    {
        try
        {
            await chatService.DeleteChatAsync(chatId, callerId, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
    }
}