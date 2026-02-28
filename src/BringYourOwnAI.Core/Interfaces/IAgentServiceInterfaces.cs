using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BringYourOwnAI.Core.Models;

namespace BringYourOwnAI.Core.Interfaces
{
    public interface IAiProvider
    {
        string Name { get; }
        Task<string> CompleteAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default);
        IAsyncEnumerable<string> StreamAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default);
        Task<ToolCallResponse> ExecuteToolCallsAsync(IEnumerable<ChatMessage> messages, IEnumerable<ToolDefinition> tools, CancellationToken cancellationToken = default);
    }

    public interface IAgentOrchestrator
    {
        Task RunAsync(string goal, AgentContext context, CancellationToken cancellationToken = default);
        event EventHandler<AgentProgressEventArgs> ProgressChanged;
    }

    public class AgentContext
    {
        public Conversation Conversation { get; set; } = null!;
        public AgentTask CurrentTask { get; set; } = null!;
        public IEnumerable<MemorySnippet> RelevantMemories { get; set; } = new List<MemorySnippet>();
        public string ActiveFilePath { get; set; } = string.Empty;
        public string ActiveFileContent { get; set; } = string.Empty;
    }

    public class ToolDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public object Parameters { get; set; } = null!; // JSON schema
    }

    public class ToolCallResponse
    {
        public string Message { get; set; } = string.Empty;
        public List<ToolCall> ToolCalls { get; set; } = new List<ToolCall>();
    }

    public class ToolCall
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty; // JSON string
    }

    public class AgentProgressEventArgs : EventArgs
    {
        public string Status { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
    }
}
