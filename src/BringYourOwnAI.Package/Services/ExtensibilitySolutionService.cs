using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.ProjectSystem.Query;
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

        private async Task<string?> GetSolutionDirectoryAsync()
        {
            try
            {
                var solutions = await _extensibility.Workspaces().QuerySolutionAsync(s => s, default);
                var activeSolution = solutions.FirstOrDefault();
                if (activeSolution != null && !string.IsNullOrEmpty(activeSolution.Path))
                {
                    return Path.GetDirectoryName(activeSolution.Path);
                }
            }
            catch
            {
                // Ignore API failures during initialization or unsupported contexts
            }
            return null;
        }

        public async Task<IEnumerable<string>> GetSolutionFilesAsync()
        {
            var dir = await GetSolutionDirectoryAsync();
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return Enumerable.Empty<string>();

            try
            {
                var files = Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories)
                    .Where(f => !f.Contains("\\obj\\") && !f.Contains("\\bin\\") && !f.Contains("\\.git\\"));
                return files;
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        public async Task<IEnumerable<string>> SearchFilesAsync(string query)
        {
            var files = await GetSolutionFilesAsync();
            var results = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    var lines = await File.ReadAllLinesAsync(file);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Contains(query, StringComparison.OrdinalIgnoreCase))
                        {
                            results.Add($"{file}:{i + 1}: {lines[i].Trim()}");
                        }
                    }
                }
                catch { /* Ignore unreadable files */ }
            }

            return results.Take(50); // Cap results to prevent massive token bloat
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
