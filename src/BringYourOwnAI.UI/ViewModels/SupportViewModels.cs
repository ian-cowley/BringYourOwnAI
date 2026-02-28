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
    public partial class ConversationsViewModel : ObservableObject
    {
        private readonly IConversationService _service;
        private Conversation _selectedConversation = null!;

        [DataMember]
        public Conversation SelectedConversation
        {
            get => _selectedConversation;
            set => SetProperty(ref _selectedConversation, value);
        }

        [DataMember]
        public ObservableCollection<Conversation> Conversations { get; } = new ObservableCollection<Conversation>();

        [DataMember]
        public IAsyncCommand NewConversationCommand { get; }

        [DataMember]
        public IAsyncCommand DeleteConversationCommand { get; }

        public ConversationsViewModel(IConversationService service)
        {
            _service = service;
            NewConversationCommand = new AsyncCommand(ExecuteNewConversationAsync);
            DeleteConversationCommand = new AsyncCommand(ExecuteDeleteConversationAsync);
            
            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            var list = await _service.GetAllAsync();
            Conversations.Clear();
            foreach (var c in list) Conversations.Add(c);
        }

        private async Task ExecuteNewConversationAsync(object? parameter, CancellationToken cancellationToken)
        {
            var conv = await _service.CreateNewAsync();
            await _service.SaveAsync(conv);
            Conversations.Insert(0, conv);
            SelectedConversation = conv;
        }

        private async Task ExecuteDeleteConversationAsync(object? parameter, CancellationToken cancellationToken)
        {
            if (parameter is Conversation conversation)
            {
                await _service.DeleteAsync(conversation.Id);
                Conversations.Remove(conversation);
            }
        }
    }

    [DataContract]
    public partial class SettingsViewModel : ObservableObject
    {
        private string _openAiKey = string.Empty;
        private string _ollamaEndpoint = "http://localhost:11434";
        private string _geminiKey = string.Empty;
        private bool _autoSelectModels = true;

        [DataMember]
        public string OpenAiKey { get => _openAiKey; set => SetProperty(ref _openAiKey, value); }
        
        [DataMember]
        public string OllamaEndpoint { get => _ollamaEndpoint; set => SetProperty(ref _ollamaEndpoint, value); }

        [DataMember]
        public string GeminiKey { get => _geminiKey; set => SetProperty(ref _geminiKey, value); }

        [DataMember]
        public bool AutoSelectModels { get => _autoSelectModels; set => SetProperty(ref _autoSelectModels, value); }

        [DataMember]
        public IAsyncCommand SaveSettingsCommand { get; }

        public SettingsViewModel()
        {
            SaveSettingsCommand = new AsyncCommand((p, c) => 
            {
                // Persistence logic here
                return Task.CompletedTask;
            });
        }
    }
}
