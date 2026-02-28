using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using BringYourOwnAI.Core.Interfaces;
using BringYourOwnAI.Core.Models;

namespace BringYourOwnAI.Providers.Common
{
    public abstract class BaseProvider : IAiProvider
    {
        protected readonly HttpClient HttpClient;
        protected readonly JsonSerializerOptions JsonOptions;

        protected BaseProvider(HttpClient httpClient)
        {
            HttpClient = httpClient;
            JsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public abstract string Name { get; }

        public abstract Task<string> CompleteAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default);

        /// <summary>
        /// Default streaming implementation wraps CompleteAsync in a single-item async enumerable.
        /// Override in subclasses that support true server-sent-event streaming.
        /// Avoids async-iterator state machines (AsyncIteratorStateMachineAttribute) that require
        /// Microsoft.Bcl.AsyncInterfaces to be available at runtime under .NET Framework 4.8.
        /// </summary>
        public virtual IAsyncEnumerable<string> StreamAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default)
        {
            return new SingleItemAsyncEnumerable(CompleteAsync(messages, cancellationToken));
        }

        public abstract Task<ToolCallResponse> ExecuteToolCallsAsync(IEnumerable<ChatMessage> messages, IEnumerable<ToolDefinition> tools, CancellationToken cancellationToken = default);

        // ------------------------------------------------------------------
        // Lightweight IAsyncEnumerable wrapper – no async-iterator state machine
        // ------------------------------------------------------------------
        private sealed class SingleItemAsyncEnumerable : IAsyncEnumerable<string>
        {
            private readonly Task<string> _task;
            public SingleItemAsyncEnumerable(Task<string> task) => _task = task;
            public IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => new Enumerator(_task);

            private sealed class Enumerator : IAsyncEnumerator<string>
            {
                private readonly Task<string> _task;
                private bool _done;
                public Enumerator(Task<string> task) => _task = task;
                public string Current { get; private set; } = string.Empty;
                public async ValueTask<bool> MoveNextAsync()
                {
                    if (_done) return false;
                    Current = await _task.ConfigureAwait(false);
                    _done = true;
                    return true;
                }
                public ValueTask DisposeAsync() => default;
            }
        }

        protected async Task<T> PostAsync<T>(string url, object payload, string apiKey, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            if (!string.IsNullOrEmpty(apiKey))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }
            
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await HttpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseJson, JsonOptions);
        }
    }
}
