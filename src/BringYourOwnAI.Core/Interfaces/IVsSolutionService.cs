using System.Collections.Generic;
using System.Threading.Tasks;

namespace BringYourOwnAI.Core.Interfaces
{
    public interface IVsSolutionService
    {
        Task<IEnumerable<string>> GetSolutionFilesAsync();
        Task<IEnumerable<string>> SearchFilesAsync(string query);
        Task<string> ReadFileAsync(string path);
        Task WriteFileAsync(string path, string content);
        Task<string> GetActiveDocumentContextAsync();
        Task RunBuildAsync();
    }
}
