using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BringYourOwnAI.Core.Interfaces;
using BringYourOwnAI.Core.Models;

namespace BringYourOwnAI.Core.Services
{
    public class ConversationService : IConversationService
    {
        private readonly string _basePath;
        private readonly JsonSerializerOptions _options;

        public ConversationService()
        {
            _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BringYourOwnAI", "conversations");
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }

            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IEnumerable<Conversation>> GetAllAsync()
        {
            var conversations = new List<Conversation>();
            var files = Directory.GetFiles(_basePath, "*.json");

            foreach (var file in files)
            {
                try
                {
                    var json = await Task.Run(() => File.ReadAllText(file));
                    var conv = JsonSerializer.Deserialize<Conversation>(json, _options);
                    if (conv != null) conversations.Add(conv);
                }
                catch { /* Ignore corrupted files */ }
            }

            return conversations.OrderByDescending(c => c.CreatedAt);
        }

        public async Task<Conversation?> GetByIdAsync(string id)
        {
            var path = Path.Combine(_basePath, $"{id}.json");
            if (!File.Exists(path)) return null;

            var json = await Task.Run(() => File.ReadAllText(path));
            return JsonSerializer.Deserialize<Conversation>(json, _options);
        }

        public async Task SaveAsync(Conversation conversation)
        {
            var path = Path.Combine(_basePath, $"{conversation.Id}.json");
            var json = JsonSerializer.Serialize(conversation, _options);
            await Task.Run(() => File.WriteAllText(path, json));
        }

        public async Task DeleteAsync(string id)
        {
            var path = Path.Combine(_basePath, $"{id}.json");
            if (File.Exists(path))
            {
                await Task.Run(() => File.Delete(path));
            }
        }

        public Task<Conversation> CreateNewAsync()
        {
            var conv = new Conversation();
            return Task.FromResult(conv);
        }
    }
}
