using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using BringYourOwnAI.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BringYourOwnAI.UI.ViewModels
{
    [DataContract]
    public class LogsViewModel : ObservableObject
    {
        private readonly ILogService _logService;

        public LogsViewModel(ILogService logService)
        {
            _logService = logService;
        }

        [DataMember]
        public ObservableCollection<LogMessage> Logs => _logService.Logs;

        public void HookDispatcher(System.Action<System.Action> dispatcherAction)
        {
            if (_logService is BringYourOwnAI.Core.Services.InMemoryLogService inMemoryService)
            {
                inMemoryService.DispatcherAction = dispatcherAction;
            }
        }
    }
}
