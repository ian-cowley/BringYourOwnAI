using System.Collections.Generic;
using System.Threading.Tasks;
using BringYourOwnAI.Core.Models;

namespace BringYourOwnAI.Core.Interfaces
{
    public interface IMemoryService
    {
        Task<IEnumerable<MemorySnippet>> GetAllAsync();
        Task<MemorySnippet?> GetByIdAsync(string id);
        Task SaveAsync(MemorySnippet snippet);
        Task DeleteAsync(string id);
        Task<IEnumerable<MemorySnippet>> SearchAsync(string query);
    }

    public interface IConversationService
    {
        Task<IEnumerable<Conversation>> GetAllAsync();
        Task<Conversation?> GetByIdAsync(string id);
        Task SaveAsync(Conversation conversation);
        Task DeleteAsync(string id);
        Task<Conversation> CreateNewAsync();
    }
}
