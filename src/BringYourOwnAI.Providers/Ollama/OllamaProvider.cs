using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BringYourOwnAI.Core.Models;
using BringYourOwnAI.Providers.Common;
using BringYourOwnAI.Providers.OpenAI;

namespace BringYourOwnAI.Providers.Ollama
{
    public class OllamaProvider : OpenAiProvider
    {
        // Ollama supports the OpenAI API format at /v1/
        public OllamaProvider(HttpClient httpClient, string endpoint = "http://localhost:11434", string model = "llama3", BringYourOwnAI.Core.Interfaces.ILogService? logService = null) 
            : base(httpClient, "ollama", model, endpoint, logService) // Ollama usually doesn't need an API key
        {
        }

        public override string Name => "Ollama (Local)";
    }
}
