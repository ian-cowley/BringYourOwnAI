using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Extensibility.UI;
using BringYourOwnAI.Core.Interfaces;
using BringYourOwnAI.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BringYourOwnAI.UI.ViewModels
{
    [DataContract]
    [KnownType(typeof(ChatViewModel))]
    [KnownType(typeof(ConversationsViewModel))]
    [KnownType(typeof(SettingsViewModel))]
    [KnownType(typeof(MemoryViewModel))]
    [KnownType(typeof(LogsViewModel))]
    public partial class MainViewModel : ObservableObject
    {
        private bool _isBusy;
        private bool _isChatActive = true;
        private bool _isSettingsActive;
        private bool _isMemoryActive;
        private bool _isLogsActive;

        [DataMember] public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
        [DataMember] 
        public bool IsChatActive 
        { 
            get => _isChatActive; 
            set 
            {
                if (SetProperty(ref _isChatActive, value) && value)
                {
                    IsSettingsActive = false;
                    IsMemoryActive = false;
                    IsLogsActive = false;
                    CurrentView = Chat;
                }
            }
        }

        [DataMember] 
        public bool IsSettingsActive 
        { 
            get => _isSettingsActive; 
            set 
            {
                if (SetProperty(ref _isSettingsActive, value) && value)
                {
                    IsChatActive = false;
                    IsMemoryActive = false;
                    IsLogsActive = false;
                    CurrentView = Settings;
                }
            }
        }

        [DataMember] 
        public bool IsMemoryActive 
        { 
            get => _isMemoryActive; 
            set 
            {
                if (SetProperty(ref _isMemoryActive, value) && value)
                {
                    IsChatActive = false;
                    IsSettingsActive = false;
                    IsLogsActive = false;
                    CurrentView = Memory;
                }
            }
        }

        [DataMember] 
        public bool IsLogsActive 
        { 
            get => _isLogsActive; 
            set 
            {
                if (SetProperty(ref _isLogsActive, value) && value)
                {
                    IsChatActive = false;
                    IsSettingsActive = false;
                    IsMemoryActive = false;
                    CurrentView = Logs;
                }
            }
        }

        private ObservableObject _currentView;

        [DataMember]
        public ObservableObject CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        [DataMember] public ChatViewModel Chat { get; }
        [DataMember] public ConversationsViewModel Conversations { get; }
        [DataMember] public SettingsViewModel Settings { get; }
        [DataMember] public MemoryViewModel Memory { get; }
        [DataMember] public LogsViewModel Logs { get; }

        [DataMember] public IAsyncCommand NavigateToChatCommand { get; }
        [DataMember] public IAsyncCommand NavigateToSettingsCommand { get; }
        [DataMember] public IAsyncCommand NavigateToMemoryCommand { get; }
        [DataMember] public IAsyncCommand NavigateToLogsCommand { get; }

        public MainViewModel(ChatViewModel chat, ConversationsViewModel conversations, SettingsViewModel settings, MemoryViewModel memory, LogsViewModel logs)
        {
            Chat = chat;
            Conversations = conversations;
            Settings = settings;
            Memory = memory;
            Logs = logs;
            _currentView = chat;

            NavigateToChatCommand = new AsyncCommand((p, c) => 
            { 
                CurrentView = Chat; 
                IsChatActive = true; IsSettingsActive = false; IsMemoryActive = false; IsLogsActive = false;
                return Task.CompletedTask; 
            });
            NavigateToSettingsCommand = new AsyncCommand((p, c) => 
            { 
                CurrentView = Settings; 
                IsChatActive = false; IsSettingsActive = true; IsMemoryActive = false; IsLogsActive = false;
                return Task.CompletedTask; 
            });
            NavigateToMemoryCommand = new AsyncCommand((p, c) => 
            { 
                CurrentView = Memory; 
                IsChatActive = false; IsSettingsActive = false; IsMemoryActive = true; IsLogsActive = false;
                return Task.CompletedTask; 
            });
            NavigateToLogsCommand = new AsyncCommand((p, c) => 
            { 
                CurrentView = Logs; 
                IsChatActive = false; IsSettingsActive = false; IsMemoryActive = false; IsLogsActive = true;
                return Task.CompletedTask; 
            });
        }
    }

    [DataContract]
    public partial class ChatViewModel : ObservableObject
    {
        private readonly IAgentOrchestrator _orchestrator;
        private readonly IConversationService _conversationService;
        private readonly IVsSolutionService _vsService;
        private readonly ISettingsService _settingsService;

        private string _inputText = string.Empty;
        private bool _isBusy;
        private string _statusText = "Ready";
        private bool _isSidebarVisible = true;
        private Conversation _currentConversation;
        private string _selectedModel = "Auto Select";

        [DataMember] public string InputText { get => _inputText; set => SetProperty(ref _inputText, value); }
        [DataMember] public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
        [DataMember] public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }
        [DataMember] public bool IsSidebarVisible { get => _isSidebarVisible; set => SetProperty(ref _isSidebarVisible, value); }
        [DataMember] public Conversation CurrentConversation { get => _currentConversation; set => SetProperty(ref _currentConversation, value); }
        [DataMember] public string SelectedModel { get => _selectedModel; set => SetProperty(ref _selectedModel, value); }

        [DataMember] public ObservableCollection<ChatMessage> Messages { get; } = new ObservableCollection<ChatMessage>();
        [DataMember] public ConversationsViewModel Conversations { get; }
        [DataMember] public ObservableCollection<string> AvailableModels { get; } = new ObservableCollection<string>();

        [DataMember] public IAsyncCommand ToggleSidebarCommand { get; }
        [DataMember] public IAsyncCommand SendMessageCommand { get; }

        public ChatViewModel(IAgentOrchestrator orchestrator, IConversationService conversationService, IVsSolutionService vsService, ConversationsViewModel conversations, ISettingsService settingsService)
        {
            _orchestrator = orchestrator;
            _conversationService = conversationService;
            _vsService = vsService;
            _settingsService = settingsService;
            Conversations = conversations;
            _orchestrator.ProgressChanged += (s, e) => StatusText = $"{e.Status}: {e.Detail}";
            
            Conversations.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(Conversations.SelectedConversation) && Conversations.SelectedConversation != null)
                {
                    await LoadConversationAsync(Conversations.SelectedConversation.Id);
                }
            };

            ToggleSidebarCommand = new AsyncCommand((p, c) => { IsSidebarVisible = !IsSidebarVisible; return Task.CompletedTask; });
            SendMessageCommand = new AsyncCommand(ExecuteSendMessageAsync);

            _currentConversation = new Conversation
            {
                Id = Guid.NewGuid().ToString(),
                Title = "New Chat",
                CreatedAt = DateTime.UtcNow
            };
            
            _ = InitialiseSettingsAsync();
            _ = InitialiseConversationAsync();
        }

        private async Task InitialiseSettingsAsync()
        {
            var settings = await _settingsService.LoadAsync();
            AvailableModels.Clear();
            AvailableModels.Add("Auto Select");
            if (settings.Providers != null)
            {
                foreach (var provider in settings.Providers)
                {
                    AvailableModels.Add(provider.Name);
                }
            }
            SelectedModel = "Auto Select";
        }

        private async Task InitialiseConversationAsync()
        {
            try
            {
                var conv = await _conversationService.CreateNewAsync();
                CurrentConversation = conv;
                Conversations.Conversations.Insert(0, conv);
            }
            catch
            {
                // Keeping placeholder temp
            }
        }

        private async Task ExecuteSendMessageAsync(object? parameter, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(InputText)) return;

            var userMessage = new ChatMessage { Role = "user", Content = InputText };
            Messages.Add(userMessage);
            CurrentConversation.Messages.Add(userMessage);
            
            var textToClear = InputText;
            InputText = string.Empty;
            IsBusy = true;

            try
            {
                var context = new AgentContext
                {
                    Conversation = CurrentConversation,
                    ActiveFilePath = await _vsService.GetActiveDocumentContextAsync()
                };

                await _orchestrator.RunAsync(textToClear, context, SelectedModel, cancellationToken);
                UpdateMessages();
            }
            catch (Exception ex)
            {
                CurrentConversation.Messages.Add(new ChatMessage 
                { 
                    Role = "assistant", 
                    Content = $"An error occurred during API execution. The model may not support the requested features (e.g. tool calling). Error details: {ex.Message}" 
                });
                UpdateMessages();
            }
            finally
            {
                IsBusy = false;
                StatusText = "Ready";
            }
        }

        private bool CanSendMessage() => !IsBusy && CurrentConversation != null;

        private void UpdateMessages()
        {
            Messages.Clear();
            foreach (var m in CurrentConversation.Messages) Messages.Add(m);
        }

        public async Task LoadConversationAsync(string id)
        {
            CurrentConversation = await _conversationService.GetByIdAsync(id) ?? await _conversationService.CreateNewAsync();
            UpdateMessages();
        }
    }
}
