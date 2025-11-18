using ChatApp.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]

public class MessageController : ControllerBase
{
    private readonly AppDbContext _db;
    public MessageController(AppDbContext db) { _db = db; }

    [HttpGet("{chatId}")]
    public async Task<IActionResult> GetHistory(Guid chatId)
    {
        var messages = await _db.Messages
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        return Ok(messages);
    }
}