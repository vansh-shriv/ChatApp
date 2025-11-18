using ChatApp.Api.Data;
using ChatApp.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly AppDbContext _db;
    public ChatController(AppDbContext db) { _db = db; }

    // get the authenticated user id from the ClaimsPrincipal per-request
    private Guid GetUserId()
    {
        var id = User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var guid))
            throw new InvalidOperationException("Authenticated user id not found.");
        return guid;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateChat(CreateChatDto dto)
    {
        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            IsGroup = dto.IsGroup
        };

        _db.Chats.Add(chat);

        foreach (var uid in dto.Members)
        {
            _db.ChatMembers.Add(new ChatMember
            {
                ChatId = chat.Id,
                UserId = uid
            });
        }

        _db.ChatMembers.Add(new ChatMember{
            ChatId = chat.Id,
            UserId = GetUserId()
        });

        await _db.SaveChangesAsync();
        return Ok(chat);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyChats()
    {
        var userId = GetUserId();
        var chats = await _db.ChatMembers
            .Where(cm => cm.UserId == userId)
            .Select(cm => cm.Chat)
            .ToListAsync();

        return Ok(chats);
    }
}

public record CreateChatDto(string Name, bool IsGroup, List<Guid> Members);