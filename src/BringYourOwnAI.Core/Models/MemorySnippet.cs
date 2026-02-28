using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BringYourOwnAI.Core.Models
{
    [DataContract]
    public class MemorySnippet
    {
        [DataMember] public string Id { get; set; } = Guid.NewGuid().ToString();
        [DataMember] public string Title { get; set; } = string.Empty;
        [DataMember] public string Content { get; set; } = string.Empty;
        [DataMember] public List<string> Tags { get; set; } = new List<string>();
        [DataMember] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [DataMember] public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
        [DataMember] public string FilePath { get; set; } = string.Empty;
    }
}
