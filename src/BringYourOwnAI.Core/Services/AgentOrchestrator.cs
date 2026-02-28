using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BringYourOwnAI.Core.Interfaces;
using BringYourOwnAI.Core.Models;

namespace BringYourOwnAI.Core.Services
{
    public class AgentOrchestrator : IAgentOrchestrator
    {
        private readonly IProviderFactory _providerFactory;
        private readonly ISettingsService _settingsService;
        private readonly IVsSolutionService _vsService;
        private readonly IMemoryService _memoryService;

        public event EventHandler<AgentProgressEventArgs>? ProgressChanged;

        public AgentOrchestrator(IProviderFactory providerFactory, ISettingsService settingsService, IVsSolutionService vsService, IMemoryService memoryService)
        {
            _providerFactory = providerFactory;
            _settingsService = settingsService;
            _vsService = vsService;
            _memoryService = memoryService;
        }

        public async Task RunAsync(string goal, AgentContext context, string? providerName = null, CancellationToken cancellationToken = default)
        {
            OnProgress("Starting Agent", $"Goal: {goal}");

            var settings = await _settingsService.LoadAsync();
            ProviderSetting? providerSetting = null;
            if (!string.IsNullOrEmpty(providerName) && providerName != "Auto Select")
            {
                providerSetting = settings.Providers.FirstOrDefault(p => p.Name == providerName);
            }
            if (providerSetting == null && settings.Providers.Any())
            {
                providerSetting = settings.Providers.First();
            }
            if (providerSetting == null)
            {
                providerSetting = new ProviderSetting { ProviderType = "openai", ApiKey = "mock", Model = "gpt-4o" };
            }

            var aiProvider = _providerFactory.CreateProvider(providerSetting);

            // 1. Initial Planning
            context.CurrentTask = new AgentTask { Title = "Task Plan" };
            var planMessage = new ChatMessage 
            { 
                Role = "system", 
                Content = $"Context: You are an AI assistant in Visual Studio. Your goal is: {goal}. " +
                          "Create a task list of steps to achieve this. Response should be a JSON array of task descriptions." 
            };
            
            OnProgress("Planning", "Asking AI for a plan...");
            var planJson = await aiProvider.CompleteAsync(new[] { planMessage }, cancellationToken);
            try
            {
                var tasks = JsonSerializer.Deserialize<List<string>>(planJson);
                if (tasks != null)
                {
                    foreach (var t in tasks) context.CurrentTask.Items.Add(new AgentTaskItem { Description = t });
                }
            }
            catch { /* Fallback if AI doesn't return JSON */ }

            // 2. Execution Loop
            int iterations = 0;
            const int maxIterations = 10;

            while (iterations < maxIterations && !cancellationToken.IsCancellationRequested)
            {
                iterations++;
                OnProgress("Thinking", $"Iteration {iterations}...");

                var tools = GetAvailableTools();
                var messages = context.Conversation.Messages.ToList();
                // Add system prompt with latest context
                messages.Insert(0, new ChatMessage { Role = "system", Content = BuildSystemPrompt(context) });

                var response = await aiProvider.ExecuteToolCallsAsync(messages, tools, cancellationToken);
                
                context.Conversation.Messages.Add(new ChatMessage { Role = "assistant", Content = response.Message });

                if (!response.ToolCalls.Any())
                {
                    OnProgress("Done", "Goal achieved or no more tools needed.");
                    break;
                }

                foreach (var toolCall in response.ToolCalls)
                {
                    OnProgress("Executing Tool", toolCall.Name);
                    var result = await DispatchToolAsync(toolCall, context);
                    context.Conversation.Messages.Add(new ChatMessage 
                    { 
                        Role = "tool", 
                        ToolCallId = toolCall.Id, 
                        Content = result 
                    });
                }
            }
        }

        private void OnProgress(string status, string detail)
        {
            ProgressChanged?.Invoke(this, new AgentProgressEventArgs { Status = status, Detail = detail });
        }

        private string BuildSystemPrompt(AgentContext context)
        {
            return $"Current Task State: {JsonSerializer.Serialize(context.CurrentTask)}\n" +
                   $"Relevant Memories: {string.Join("\n", context.RelevantMemories.Select(m => m.Content))}\n" +
                   $"Active File: {context.ActiveFilePath}";
        }

        private IEnumerable<ToolDefinition> GetAvailableTools()
        {
            yield return new ToolDefinition { Name = "read_file", Description = "Read file content", Parameters = new { path = "string" } };
            yield return new ToolDefinition { Name = "write_file", Description = "Write file content", Parameters = new { path = "string", content = "string" } };
            yield return new ToolDefinition { Name = "list_files", Description = "List solution files" };
            yield return new ToolDefinition { Name = "search_code", Description = "Search codebase for string", Parameters = new { query = "string" } };
            yield return new ToolDefinition { Name = "run_build", Description = "Run solution build" };
        }

        private async Task<string> DispatchToolAsync(ToolCall toolCall, AgentContext context)
        {
            try
            {
                var args = JsonDocument.Parse(toolCall.Arguments).RootElement;
                switch (toolCall.Name)
                {
                    case "read_file":
                        return await _vsService.ReadFileAsync(args.GetProperty("path").GetString() ?? string.Empty);
                    case "write_file":
                        await _vsService.WriteFileAsync(args.GetProperty("path").GetString() ?? string.Empty, args.GetProperty("content").GetString() ?? string.Empty);
                        return "File written successfully.";
                    case "list_files":
                        var files = await _vsService.GetSolutionFilesAsync();
                        return string.Join("\n", files);
                    case "search_code":
                        var results = await _vsService.SearchFilesAsync(args.GetProperty("query").GetString() ?? string.Empty);
                        return results.Any() ? string.Join("\n", results) : "No results found.";
                    case "run_build":
                        await _vsService.RunBuildAsync();
                        return "Build started.";
                    default:
                        return $"Unknown tool: {toolCall.Name}";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
