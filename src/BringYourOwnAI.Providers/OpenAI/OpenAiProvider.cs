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
        private readonly string _baseUrl;

        public OpenAiProvider(HttpClient httpClient, string apiKey, string model = "gpt-4o", string? endpoint = null, ILogService? logService = null) : base(httpClient, logService)
        {
            _apiKey = apiKey;
            _model = model;
            
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                _baseUrl = "https://api.openai.com/v1/chat/completions";
            }
            else
            {
                string normEndpoint = endpoint.TrimEnd('/');
                if (!normEndpoint.EndsWith("/v1", StringComparison.OrdinalIgnoreCase) && 
                    !normEndpoint.ToLowerInvariant().Contains("/v1/") &&
                    !normEndpoint.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
                {
                    normEndpoint += "/v1";
                }
                
                _baseUrl = normEndpoint.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase) 
                    ? normEndpoint 
                    : normEndpoint + "/chat/completions";
            }
        }

        public override string Name => "OpenAI";

        private async Task<string> ResolveModelAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(_model))
            {
                return _model;
            }

            try
            {
                var modelsUrl = _baseUrl.Replace("/chat/completions", "/models");
                using var request = new HttpRequestMessage(HttpMethod.Get, modelsUrl);
                if (!string.IsNullOrEmpty(_apiKey) && _apiKey != "mock" && _apiKey != "dummy")
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
                }

                var response = await HttpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var json = JsonDocument.Parse(content);
                    if (json.RootElement.TryGetProperty("data", out var data) && data.GetArrayLength() > 0)
                    {
                        var firstModel = data[0].GetProperty("id").GetString();
                        if (!string.IsNullOrEmpty(firstModel))
                        {
                            return firstModel;
                        }
                    }
                }
            }
            catch
            {
                // Fallback to a default if fetching fails or endpoint doesn't support /models
            }

            return "gpt-3.5-turbo"; // absolute fallback
        }

        public override async Task<string> CompleteAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default)
        {
            var modelToUse = await ResolveModelAsync(cancellationToken);
            var payload = new
            {
                model = modelToUse,
                messages = messages.Select(m => new { role = m.Role, content = m.Content })
            };

            var response = await PostAsync<JsonElement>(_baseUrl, payload, _apiKey, cancellationToken);
            return response.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()!;
        }

        public override async Task<ToolCallResponse> ExecuteToolCallsAsync(IEnumerable<ChatMessage> messages, IEnumerable<ToolDefinition> tools, CancellationToken cancellationToken = default)
        {
            var modelToUse = await ResolveModelAsync(cancellationToken);
            var payload = new
            {
                model = modelToUse,
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

            try
            {
                var responseJson = await PostAsync<JsonElement>(_baseUrl, payload, _apiKey, cancellationToken);

                var message = responseJson.GetProperty("choices")[0].GetProperty("message");
                var content = message.TryGetProperty("content", out var c) ? c.GetString() ?? string.Empty : string.Empty;

                var toolCalls = new List<ToolCall>();
                if (message.TryGetProperty("tool_calls", out var tcArray))
                {
                    foreach (var tc in tcArray.EnumerateArray())
                    {
                        if (tc.GetProperty("type").GetString() == "function")
                        {
                            var function = tc.GetProperty("function");
                            toolCalls.Add(new ToolCall
                            {
                                Id = tc.GetProperty("id").GetString() ?? Guid.NewGuid().ToString(),
                                Name = function.GetProperty("name").GetString() ?? string.Empty,
                                Arguments = function.GetProperty("arguments").GetString() ?? string.Empty
                            });
                        }
                    }
                }

                return new ToolCallResponse
                {
                    Message = content,
                    ToolCalls = toolCalls
                };
            }
            catch (System.Net.Http.HttpRequestException ex) when (ex.Message.Contains("400"))
            {
                // Many local models reject the strict `tools` JSON schema specification.
                // We gracefully catch the 400 Bad Request and fallback to a standard textual Completion request.
                LogService?.Log(Core.Interfaces.LogLevel.Warning, "Model rejected Tools payload (400 Bad Request). Falling back to standard completion.", ex.Message);
                
                var fallbackMessage = await CompleteAsync(messages, cancellationToken);
                return new ToolCallResponse
                {
                    Message = fallbackMessage,
                    ToolCalls = new List<ToolCall>()
                };
            }
        }
    }
}
