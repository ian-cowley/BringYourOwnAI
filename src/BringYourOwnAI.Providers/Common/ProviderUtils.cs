using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BringYourOwnAI.Core.Interfaces;
using BringYourOwnAI.Core.Models;
using BringYourOwnAI.Providers.Google;
using BringYourOwnAI.Providers.Ollama;
using BringYourOwnAI.Providers.OpenAI;

namespace BringYourOwnAI.Providers.Common
{
    public class ProviderFactory
    {
        private readonly HttpClient _httpClient;

        public ProviderFactory(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IAiProvider CreateProvider(ProviderConfig config)
        {
            switch (config.Type.ToLower())
            {
                case "openai": return new OpenAiProvider(_httpClient, config.ApiKey, config.Model);
                case "ollama": return new OllamaProvider(_httpClient, config.Endpoint, config.Model);
                case "gemini": return new GoogleGeminiProvider(_httpClient, config.ApiKey, config.Model);
                default: throw new NotSupportedException($"Provider type {config.Type} not supported.");
            }
        }
    }

    public class ProviderConfig
    {
        public string Type { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
    }

    public class AutoModelSelector
    {
        private readonly ProviderFactory _factory;
        private readonly ProviderConfig _baseConfig;

        public AutoModelSelector(ProviderFactory factory, ProviderConfig baseConfig)
        {
            _factory = factory;
            _baseConfig = baseConfig;
        }

        public async Task<IAiProvider> SelectBestProviderAsync(string goal, CancellationToken cancellationToken = default)
        {
            // Implementation of model routing
            // For now, return the base provider, but in v1 this would use a 'router' model
            return _factory.CreateProvider(_baseConfig);
        }
    }
}
