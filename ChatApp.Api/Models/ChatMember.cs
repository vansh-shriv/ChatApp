using System;

namespace ChatApp.Api.Models;

public class ChatMember {
    public Guid ChatId { get; set; }
    public Chat? Chat { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
}