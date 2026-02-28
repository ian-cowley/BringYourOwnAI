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

namespace BringYourOwnAI.Providers.Google
{
    public class GoogleGeminiProvider : BaseProvider
    {
        private readonly string _apiKey;
        private readonly string _model;
        private const string BaseUrlPattern = "https://generativelanguage.googleapis.com/v1beta/models/{0}:generateContent?key={1}";

        public GoogleGeminiProvider(HttpClient httpClient, string apiKey, string model = "gemini-1.5-pro", ILogService? logService = null) : base(httpClient, logService)
        {
            _apiKey = apiKey;
            _model = model;
        }

        public override string Name => "Google Gemini";

        public override async Task<string> CompleteAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default)
        {
            var url = string.Format(BaseUrlPattern, _model, _apiKey);
            var payload = new
            {
                contents = messages.Select(m => new
                {
                    role = m.Role == "assistant" ? "model" : "user",
                    parts = new[] { new { text = m.Content } }
                })
            };

            var response = await PostAsync<JsonElement>(url, payload, string.Empty, cancellationToken);
            return response.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString()!;
        }

        public override async Task<ToolCallResponse> ExecuteToolCallsAsync(IEnumerable<ChatMessage> messages, IEnumerable<ToolDefinition> tools, CancellationToken cancellationToken = default)
        {
            // Gemini has a different tool format, but for this MVP we'll just implement simple chat
            // In a real version, we'd translate tools to Gemini's 'function_declarations'
            return new ToolCallResponse { Message = await CompleteAsync(messages, cancellationToken) };
        }
    }
}
