using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BringYourOwnAI.Core.Interfaces;
using BringYourOwnAI.Core.Models;
using BringYourOwnAI.Providers.Common;

namespace BringYourOwnAI.Providers.OpenAI
{
    public class OpenAiProvider : BaseProvider
    {
        private readonly string _apiKey;
        private readonly string _model;
        private const string BaseUrl = "https://api.openai.com/v1/chat/completions";

        public OpenAiProvider(HttpClient httpClient, string apiKey, string model = "gpt-4o") : base(httpClient)
        {
            _apiKey = apiKey;
            _model = model;
        }

        public override string Name => "OpenAI";

        public override async Task<string> CompleteAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                model = _model,
                messages = messages.Select(m => new { role = m.Role, content = m.Content })
            };

            var response = await PostAsync<JsonElement>(BaseUrl, payload, _apiKey, cancellationToken);
            return response.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }

        public override async Task<ToolCallResponse> ExecuteToolCallsAsync(IEnumerable<ChatMessage> messages, IEnumerable<ToolDefinition> tools, CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                model = _model,
                messages = messages.Select(m => new { role = m.Role, content = m.Content }),
                tools = tools.Select(t => new
                {
                    type = "function",
                    function = new
                    {
                        name = t.Name,
                        description = t.Description,
                        parameters = t.Parameters
                    }
                }),
                tool_choice = "auto"
            };

            var response = await PostAsync<JsonElement>(BaseUrl, payload, _apiKey, cancellationToken);
            var choice = response.GetProperty("choices")[0];
            var message = choice.GetProperty("message");

            var result = new ToolCallResponse
            {
                Message = message.TryGetProperty("content", out var content) ? content.GetString() : string.Empty
            };

            if (message.TryGetProperty("tool_calls", out var toolCalls))
            {
                foreach (var toolCall in toolCalls.EnumerateArray())
                {
                    result.ToolCalls.Add(new ToolCall
                    {
                        Id = toolCall.GetProperty("id").GetString(),
                        Name = toolCall.GetProperty("function").GetProperty("name").GetString(),
                        Arguments = toolCall.GetProperty("function").GetProperty("arguments").GetString()
                    });
                }
            }

            return result;
        }
    }
}
