using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.ChatService;

namespace PussyCats.Web.Controllers;

public class ChatController : Controller
{
    private readonly IChatService chat;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public ChatController(IChatService chat)
    {
        this.chat = chat;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var chats = await chat.GetChatsForUserAsync(CurrentUserId, cancellationToken);
        return View(chats);
    }

    public async Task<IActionResult> Show(int id, CancellationToken cancellationToken)
    {
        var messages = await chat.GetMessagesAsync(id, CurrentUserId, cancellationToken);
        await chat.MarkMessagesAsReadAsync(id, CurrentUserId, cancellationToken);
        ViewBag.ChatId = id;
        ViewBag.CurrentUserId = CurrentUserId;
        return View(messages);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int id, string content, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(content))
            await chat.SendMessageAsync(id, content, CurrentUserId, MessageType.Text, cancellationToken);
        return RedirectToAction(nameof(Show), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await chat.DeleteChatAsync(id, CurrentUserId, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(int id, CancellationToken cancellationToken)
    {
        await chat.BlockChatAsync(id, CurrentUserId, cancellationToken);
        return RedirectToAction(nameof(Show), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unblock(int id, CancellationToken cancellationToken)
    {
        await chat.UnblockChatAsync(id, CurrentUserId, cancellationToken);
        return RedirectToAction(nameof(Show), new { id });
    }
}
