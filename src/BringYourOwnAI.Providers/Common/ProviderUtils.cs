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
    public class ProviderFactory : IProviderFactory
    {
        private readonly HttpClient _httpClient;
        private readonly ILogService? _logService;

        public ProviderFactory(HttpClient httpClient, ILogService? logService = null)
        {
            _httpClient = httpClient;
            _logService = logService;
        }

        public IAiProvider CreateProvider(ProviderSetting config)
        {
            var type = config.ProviderType?.ToLower() ?? "unknown";
            
            if (type == "openai" || type == "lmstudio")
            {
                return new OpenAiProvider(_httpClient, config.ApiKey, config.Model ?? "gpt-4o", config.Endpoint, _logService);
            }
            if (type == "ollama")
            {
                return new OllamaProvider(_httpClient, config.Endpoint, config.Model ?? "llama3", _logService);
            }
            if (type == "gemini")
            {
                return new GoogleGeminiProvider(_httpClient, config.ApiKey, config.Model ?? "gemini-pro", _logService);
            }

            throw new NotSupportedException($"Provider type {config.ProviderType} not supported.");
        }
    }

    public class AutoModelSelector
    {
        private readonly ProviderFactory _factory;
        private readonly ProviderSetting _baseConfig;

        public AutoModelSelector(ProviderFactory factory, ProviderSetting baseConfig)
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
