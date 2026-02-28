using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Extensibility;
using BringYourOwnAI.Core.Interfaces;

namespace BringYourOwnAI.Package.Services
{
    public class ExtensibilitySolutionService : IVsSolutionService
    {
        private readonly VisualStudioExtensibility _extensibility;

        public ExtensibilitySolutionService(VisualStudioExtensibility extensibility)
        {
            _extensibility = extensibility;
        }

        public Task<IEnumerable<string>> GetSolutionFilesAsync()
        {
            return Task.FromResult<IEnumerable<string>>(new List<string>());
        }

        public async Task<string> ReadFileAsync(string path)
        {
            if (File.Exists(path))
            {
                return await File.ReadAllTextAsync(path);
            }
            return string.Empty;
        }

        public async Task WriteFileAsync(string path, string content)
        {
            await File.WriteAllTextAsync(path, content);
        }

        public Task<string> GetActiveDocumentContextAsync()
        {
            // Requires IClientContext in Extensibility 17.10 API, returning empty for MVP
            return Task.FromResult(string.Empty);
        }

        public Task RunBuildAsync()
        {
            return Task.CompletedTask; 
        }
    }
}
