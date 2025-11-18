using System;
using System.Collections.Generic;

namespace ChatApp.Api.Models;

public class Chat
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public bool IsGroup { get; set; }

    public ICollection<ChatMember> Members { get; set; } = new List<ChatMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
