using System;
using System.Collections.Generic;

namespace BringYourOwnAI.Core.Models
{
    public class AgentTask
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public TaskStatus Status { get; set; } = TaskStatus.Todo;
        public List<AgentTaskItem> Items { get; set; } = new List<AgentTaskItem>();
    }

    public class AgentTaskItem
    {
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public string Result { get; set; } = string.Empty;
    }

    public enum TaskStatus
    {
        Todo,
        InProgress,
        Completed,
        Failed
    }
}
