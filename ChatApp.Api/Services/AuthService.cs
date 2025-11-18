// Create password hashing (HMACSHA512) and JWT generation function using JwtSecurityTokenHandler.
// Key behavior:
// Register(username, password) → create salt/hash and save user.
// Authenticate(username, password) → verify and return JWT.

using ChatApp.Api.Models;
using ChatApp.Api.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System;
using Microsoft.Extensions.Configuration;

namespace ChatApp.Api.Services;

public interface IAuthService{
    void CreatePasswordHash(string password, out byte[] hash, out byte[] salt);
    bool VerifyPassword(string password, byte[] hash, byte[] salt);
    string CreateToken(User user);
}

public class AuthService : IAuthService{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;

    public AuthService(IConfiguration config, AppDbContext db){
        _config = config;
        _db = db;
    }

    public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt){
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    public bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt){
        using var hmac = new HMACSHA512(storedSalt);
        var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return CryptographicOperations.FixedTimeEquals(computed, storedHash);
    }

    public string CreateToken(User user){
        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var keyBytes = Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "please_change_this_key");
        var key = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}