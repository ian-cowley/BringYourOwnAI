using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Extensibility.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using BringYourOwnAI.Core.Interfaces;
using BringYourOwnAI.Core.Models;

namespace BringYourOwnAI.UI.ViewModels
{
    [DataContract]
    public partial class MemoryViewModel : ObservableObject
    {
        private readonly IMemoryService _service;
        private string _searchQuery = string.Empty;

        [DataMember]
        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        [DataMember]
        public ObservableCollection<MemorySnippet> Snippets { get; } = new ObservableCollection<MemorySnippet>();

        [DataMember]
        public IAsyncCommand SearchCommand { get; }

        [DataMember]
        public IAsyncCommand DeleteSnippetCommand { get; }

        public MemoryViewModel(IMemoryService service)
        {
            _service = service;
            SearchCommand = new AsyncCommand(ExecuteSearchAsync);
            DeleteSnippetCommand = new AsyncCommand(ExecuteDeleteSnippetAsync);
            
            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            var list = await _service.GetAllAsync();
            Snippets.Clear();
            foreach (var s in list) Snippets.Add(s);
        }

        private async Task ExecuteSearchAsync(object? parameter, CancellationToken cancellationToken)
        {
            var results = await _service.SearchAsync(SearchQuery);
            Snippets.Clear();
            foreach (var s in results) Snippets.Add(s);
        }

        private async Task ExecuteDeleteSnippetAsync(object? parameter, CancellationToken cancellationToken)
        {
            if (parameter is MemorySnippet snippet)
            {
                await _service.DeleteAsync(snippet.Id);
                Snippets.Remove(snippet);
            }
        }
    }
}
