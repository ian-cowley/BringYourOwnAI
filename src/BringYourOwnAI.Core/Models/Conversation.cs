using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BringYourOwnAI.Core.Models
{
    [DataContract]
    public class ChatMessage
    {
        [DataMember] public string Id { get; set; } = Guid.NewGuid().ToString();
        [DataMember] public string Role { get; set; } = "user"; // "user", "assistant", "system", "tool"
        [DataMember] public string Content { get; set; } = string.Empty;
        [DataMember] public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        [DataMember] public string ToolCallId { get; set; } = string.Empty;
    }

    [DataContract]
    public class Conversation
    {
        [DataMember] public string Id { get; set; } = Guid.NewGuid().ToString();
        [DataMember] public string Title { get; set; } = "New Conversation";
        [DataMember] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [DataMember] public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        [DataMember] public string ProviderName { get; set; } = string.Empty;
        [DataMember] public string ModelName { get; set; } = string.Empty;
    }
}
