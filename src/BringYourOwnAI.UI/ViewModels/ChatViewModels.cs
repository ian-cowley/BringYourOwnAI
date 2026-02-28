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
    public partial class MainViewModel : ObservableObject
    {
        private bool _isBusy;
        private bool _isChatActive = true;
        private bool _isSettingsActive;
        private bool _isMemoryActive;

        [DataMember] public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
        [DataMember] public bool IsChatActive { get => _isChatActive; set => SetProperty(ref _isChatActive, value); }
        [DataMember] public bool IsSettingsActive { get => _isSettingsActive; set => SetProperty(ref _isSettingsActive, value); }
        [DataMember] public bool IsMemoryActive { get => _isMemoryActive; set => SetProperty(ref _isMemoryActive, value); }

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

        [DataMember] public IAsyncCommand NavigateToChatCommand { get; }
        [DataMember] public IAsyncCommand NavigateToSettingsCommand { get; }
        [DataMember] public IAsyncCommand NavigateToMemoryCommand { get; }

        public MainViewModel(ChatViewModel chat, ConversationsViewModel conversations, SettingsViewModel settings, MemoryViewModel memory)
        {
            Chat = chat;
            Conversations = conversations;
            Settings = settings;
            Memory = memory;
            _currentView = chat;

            NavigateToChatCommand = new AsyncCommand((p, c) => 
            { 
                CurrentView = Chat; 
                IsChatActive = true; IsSettingsActive = false; IsMemoryActive = false;
                return Task.CompletedTask; 
            });
            NavigateToSettingsCommand = new AsyncCommand((p, c) => 
            { 
                CurrentView = Settings; 
                IsChatActive = false; IsSettingsActive = true; IsMemoryActive = false;
                return Task.CompletedTask; 
            });
            NavigateToMemoryCommand = new AsyncCommand((p, c) => 
            { 
                CurrentView = Memory; 
                IsChatActive = false; IsSettingsActive = false; IsMemoryActive = true;
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

        private string _inputText = string.Empty;
        private bool _isBusy;
        private string _statusText = "Ready";
        private bool _isSidebarVisible = true;
        private Conversation _currentConversation;

        [DataMember] public string InputText { get => _inputText; set => SetProperty(ref _inputText, value); }
        [DataMember] public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
        [DataMember] public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }
        [DataMember] public bool IsSidebarVisible { get => _isSidebarVisible; set => SetProperty(ref _isSidebarVisible, value); }
        [DataMember] public Conversation CurrentConversation { get => _currentConversation; set => SetProperty(ref _currentConversation, value); }

        [DataMember] public ObservableCollection<ChatMessage> Messages { get; } = new ObservableCollection<ChatMessage>();
        [DataMember] public ConversationsViewModel Conversations { get; }

        [DataMember] public IAsyncCommand ToggleSidebarCommand { get; }
        [DataMember] public IAsyncCommand SendMessageCommand { get; }

        public ChatViewModel(IAgentOrchestrator orchestrator, IConversationService conversationService, IVsSolutionService vsService, ConversationsViewModel conversations)
        {
            _orchestrator = orchestrator;
            _conversationService = conversationService;
            _vsService = vsService;
            Conversations = conversations;
            _orchestrator.ProgressChanged += (s, e) => StatusText = $"{e.Status}: {e.Detail}";

            ToggleSidebarCommand = new AsyncCommand((p, c) => { IsSidebarVisible = !IsSidebarVisible; return Task.CompletedTask; });
            SendMessageCommand = new AsyncCommand(ExecuteSendMessageAsync);

            _currentConversation = new Conversation
            {
                Id = Guid.NewGuid().ToString(),
                Title = "New Chat",
                CreatedAt = DateTime.UtcNow
            };
            _ = InitialiseConversationAsync();
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

                await _orchestrator.RunAsync(textToClear, context, cancellationToken);
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
