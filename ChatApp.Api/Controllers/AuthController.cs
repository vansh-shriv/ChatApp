using ChatApp.Api.Data;
using ChatApp.Api.Models;
using ChatApp.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]

public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAuthService _auth;

    public AuthController(AppDbContext db, IAuthService auth)
    {
        _db = db;
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
            return BadRequest("Username already exists.");

        _auth.CreatePasswordHash(dto.Password, out var hash, out var salt);

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = hash,
            PasswordSalt = salt
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok("User registered.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Username == dto.Username);
        if (user == null)
            return Unauthorized("Invalid username.");

        if (!_auth.VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt))
            return Unauthorized("Invalid password.");

        var token = _auth.CreateToken(user);

        return Ok(new { token, userId = user.Id, username = user.Username });
    }
}

public record RegisterDto(string Username, string Email, string Password);
public record LoginDto(string Username, string Password);