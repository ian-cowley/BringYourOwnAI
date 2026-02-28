using System;
using System.Collections.ObjectModel;
using System.Threading;
using BringYourOwnAI.Core.Interfaces;

namespace BringYourOwnAI.Core.Services
{
    public class InMemoryLogService : ILogService
    {
        private readonly object _lock = new object();
        public ObservableCollection<LogMessage> Logs { get; } = new ObservableCollection<LogMessage>();

        // Optional hook if the UI layer needs to synchronize cross-thread collection assignments
        public Action<Action>? DispatcherAction { get; set; }

        public void Log(LogLevel level, string message, string? payload = null)
        {
            var logMessage = new LogMessage
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                Payload = payload
            };

            if (DispatcherAction != null)
            {
                DispatcherAction(() =>
                {
                    lock (_lock)
                    {
                        Logs.Add(logMessage);
                    }
                });
            }
            else
            {
                lock (_lock)
                {
                    Logs.Add(logMessage);
                }
            }
        }

        public void Clear()
        {
            if (DispatcherAction != null)
            {
                DispatcherAction(() =>
                {
                    lock (_lock)
                    {
                        Logs.Clear();
                    }
                });
            }
            else
            {
                lock (_lock)
                {
                    Logs.Clear();
                }
            }
        }
    }
}
