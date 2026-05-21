using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.ChatService;
using PussyCats.Library.Services.Jobs;

namespace PussyCats.Web.Controllers;

//[Authorize]
public class ChatController : Controller
{
    private readonly IChatService _chatService;
    private readonly IJobService _jobService;
    // In a real app, this would be populated from the authenticated user's claims
    private readonly int _currentUserId; 
    private readonly AppMode _currentMode;
    private readonly int? _currentCompanyId;

    public ChatController(IChatService chatService, IJobService jobService)
    {
        _chatService = chatService;
        _jobService = jobService;
        
        // Mocking session for demonstration - in reality, use User.Claims
        _currentUserId = 1; 
        _currentMode = AppMode.Candidate;
    }

    public async Task<IActionResult> Index()
    {
        var chats = _currentMode == AppMode.Company
            ? await _chatService.GetChatsForCompanyAsync(_currentCompanyId ?? 0)
            : await _chatService.GetChatsForUserAsync(_currentUserId);

        ViewBag.Mode = _currentMode;
        return View(chats);
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages(int chatId)
    {
        var callerId = GetCallerId();
        var messages = await _chatService.GetMessagesAsync(chatId, callerId);
        await _chatService.MarkMessagesAsReadAsync(chatId, callerId);
        
        return PartialView("_MessageHistory", messages);
    }

    [HttpGet]
    public async Task<IActionResult> GetChatList(string tab = "Users")
    {
        var chats = _currentMode == AppMode.Company
            ? await _chatService.GetChatsForCompanyAsync(_currentCompanyId ?? 0)
            : await _chatService.GetChatsForUserAsync(_currentUserId);

        if (_currentMode == AppMode.Candidate)
        {
            chats = tab == "Users" 
                ? chats.Where(c => c.SecondUser != null).ToList() 
                : chats.Where(c => c.Company != null).ToList();
        }

        return PartialView("_ChatList", chats);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string query, string tab = "Users")
    {
        if (string.IsNullOrWhiteSpace(query)) return BadRequest();

        var results = new List<SearchResultViewModel>();

        if (_currentMode == AppMode.Candidate)
        {
            if (tab == "Users")
            {
                var users = await _chatService.SearchUsersAsync(query);
                results.AddRange(users.Where(u => u.UserId != _currentUserId).Select(u => new SearchResultViewModel { Id = u.UserId, Name = $"{u.FirstName} {u.LastName}", Type = "User" }));
            }
            else
            {
                var companies = await _chatService.SearchCompaniesAsync(query);
                results.AddRange(companies.Select(c => new SearchResultViewModel { Id = c.CompanyId, Name = c.CompanyName, Type = "Company" }));
            }
        }
        else
        {
            var users = await _chatService.SearchUsersAsync(query);
            results.AddRange(users.Select(u => new SearchResultViewModel { Id = u.UserId, Name = $"{u.FirstName} {u.LastName}", Type = "User" }));
        }

        return Json(results);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(int chatId, string content, MessageType type = MessageType.Text)
    {
        if (string.IsNullOrWhiteSpace(content)) return BadRequest();
        
        await _chatService.SendMessageAsync(chatId, content, GetCallerId(), type);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> BlockChat(int chatId)
    {
        await _chatService.BlockChatAsync(chatId, GetCallerId());
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> UnblockChat(int chatId)
    {
        await _chatService.UnblockChatAsync(chatId, GetCallerId());
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteChat(int chatId)
    {
        await _chatService.DeleteChatAsync(chatId, GetCallerId());
        return Ok();
    }

    private int GetCallerId() => _currentMode == AppMode.Company ? (_currentCompanyId ?? 0) : _currentUserId;
}

public class SearchResultViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
