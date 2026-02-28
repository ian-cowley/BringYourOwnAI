using System;
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
        private string _searchQuery = string.Empty;
        private System.Collections.Generic.List<Conversation> _allConversations = new System.Collections.Generic.List<Conversation>();

        [DataMember]
        public Conversation SelectedConversation
        {
            get => _selectedConversation;
            set => SetProperty(ref _selectedConversation, value);
        }

        [DataMember]
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    ApplyFilter();
                }
            }
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
            _allConversations.Clear();
            _allConversations.AddRange(list);
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            Conversations.Clear();
            foreach (var c in _allConversations)
            {
                if (string.IsNullOrWhiteSpace(SearchQuery) || c.Title.IndexOf(SearchQuery, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Conversations.Add(c);
                }
            }
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
        private readonly ISettingsService _settingsService;
        private bool _autoSelectModels = true;

        [DataMember]
        public ObservableCollection<ProviderSetting> Providers { get; } = new ObservableCollection<ProviderSetting>();

        [DataMember]
        public bool AutoSelectModels { get => _autoSelectModels; set => SetProperty(ref _autoSelectModels, value); }

        [DataMember]
        public IAsyncCommand SaveSettingsCommand { get; }

        [DataMember]
        public IAsyncCommand AddProviderCommand { get; }

        [DataMember]
        public IAsyncCommand RemoveProviderCommand { get; }

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            
            SaveSettingsCommand = new AsyncCommand(async (p, c) => 
            {
                var settings = new Settings
                {
                    Providers = new System.Collections.Generic.List<ProviderSetting>(Providers),
                    AutoSelectModels = AutoSelectModels
                };
                await _settingsService.SaveAsync(settings);
            });

            AddProviderCommand = new AsyncCommand((p, c) => 
            {
                Providers.Add(new ProviderSetting { Id = Guid.NewGuid().ToString(), Name = "New Provider", ProviderType = "openai" });
                return Task.CompletedTask;
            });

            RemoveProviderCommand = new AsyncCommand((p, c) => 
            {
                if (p is ProviderSetting provider && Providers.Contains(provider))
                {
                    Providers.Remove(provider);
                }
                return Task.CompletedTask;
            });

            _ = LoadSettingsAsync();
        }

        private async Task LoadSettingsAsync()
        {
            var settings = await _settingsService.LoadAsync();
            Providers.Clear();
            if (settings.Providers != null)
            {
                foreach (var provider in settings.Providers)
                {
                    Providers.Add(provider);
                }
            }
            AutoSelectModels = settings.AutoSelectModels;
        }
    }

}
