using System;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Api.Data;
using ChatApp.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly AppDbContext _db;
    public ChatHub(AppDbContext db)
    {
        _db = db;
    }

    public override async Task OnConnectedAsync()
    {
        var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var userId))
        {
            var chats = await _db.ChatMembers
                .Where(cm => cm.UserId == userId)
                .Select(cm => cm.ChatId)
                .ToListAsync();

            foreach (var chatId in chats)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
            }
        }

        await base.OnConnectedAsync();
    }

    public async Task JoinChat(string chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
    }

    public async Task LeaveChat(string chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
    }

    public async Task SendMessage(string chatId, string content)
    {
        var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return;

        if (!Guid.TryParse(chatId, out var chatGuid))
            return;

        var msg = new Message
        {
            Id = Guid.NewGuid(),
            ChatId = chatGuid,
            SenderId = userId,
            Content = content ?? string.Empty,
            Timestamp = DateTime.UtcNow
        };

        _db.Messages.Add(msg);
        await _db.SaveChangesAsync();

        await Clients.Group(chatId).SendAsync("ReceiveMessage", new
        {
            id = msg.Id,
            chatId = chatId,
            senderId = msg.SenderId,
            content = msg.Content,
            timestamp = msg.Timestamp
        });
    }
}

