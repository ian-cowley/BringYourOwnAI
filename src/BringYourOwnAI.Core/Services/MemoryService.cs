using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BringYourOwnAI.Core.Interfaces;
using BringYourOwnAI.Core.Models;

namespace BringYourOwnAI.Core.Services
{
    public class MemoryService : IMemoryService
    {
        private readonly string _basePath;

        public MemoryService()
        {
            _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BringYourOwnAI", "memory");
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task<IEnumerable<MemorySnippet>> GetAllAsync()
        {
            var snippets = new List<MemorySnippet>();
            var files = Directory.GetFiles(_basePath, "*.md");

            foreach (var file in files)
            {
                var snippet = await LoadFromFileAsync(file);
                if (snippet != null) snippets.Add(snippet);
            }

            return snippets;
        }

        public async Task<MemorySnippet?> GetByIdAsync(string id)
        {
            var path = Path.Combine(_basePath, $"{id}.md");
            if (!File.Exists(path)) return null;
            return await LoadFromFileAsync(path);
        }

        public async Task SaveAsync(MemorySnippet snippet)
        {
            snippet.LastModifiedAt = DateTime.UtcNow;
            var path = Path.Combine(_basePath, $"{snippet.Id}.md");
            snippet.FilePath = path;

            var sb = new StringBuilder();
            sb.AppendLine("---");
            sb.AppendLine($"id: {snippet.Id}");
            sb.AppendLine($"title: \"{snippet.Title}\"");
            sb.AppendLine($"tags: [{string.Join(", ", snippet.Tags)}]");
            sb.AppendLine($"created: {snippet.CreatedAt:yyyy-MM-dd}");
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine(snippet.Content);

            await Task.Run(() => File.WriteAllText(path, sb.ToString()));
        }

        public async Task DeleteAsync(string id)
        {
            var path = Path.Combine(_basePath, $"{id}.md");
            if (File.Exists(path))
            {
                await Task.Run(() => File.Delete(path));
            }
        }

        public async Task<IEnumerable<MemorySnippet>> SearchAsync(string query)
        {
            var all = await GetAllAsync();
            if (string.IsNullOrWhiteSpace(query)) return all;

            return all.Where(s => 
                s.Title.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 || 
                s.Content.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                s.Tags.Any(t => t.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        private async Task<MemorySnippet> LoadFromFileAsync(string path)
        {
            try
            {
                var content = await Task.Run(() => File.ReadAllText(path));
                var title = Path.GetFileNameWithoutExtension(path);
                
                // Simple YAML front matter parser
                var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var snippet = new MemorySnippet { Id = Path.GetFileNameWithoutExtension(path), FilePath = path };
                
                int endOfFrontMatter = -1;
                if (lines.Length > 0 && lines[0].Trim() == "---")
                {
                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (lines[i].Trim() == "---")
                        {
                            endOfFrontMatter = i;
                            break;
                        }

                        var parts = lines[i].Split(new[] { ':' }, 2);
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim().ToLower();
                            var val = parts[1].Trim().Trim('\"', '[', ']');

                            switch (key)
                            {
                                case "title": snippet.Title = val; break;
                                case "tags": snippet.Tags = val.Split(',').Select(t => t.Trim()).ToList(); break;
                                case "created": DateTime.TryParse(val, out var dt); snippet.CreatedAt = dt; break;
                            }
                        }
                    }
                }

                if (endOfFrontMatter != -1)
                {
                    snippet.Content = string.Join(Environment.NewLine, lines.Skip(endOfFrontMatter + 1)).Trim();
                }
                else
                {
                    snippet.Content = content;
                }

                return snippet;
            }
            catch
            {
                return null;
            }
        }
    }
}
