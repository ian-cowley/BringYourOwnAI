using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace BringYourOwnAI.Core.Interfaces
{
    public interface ILogService
    {
        ObservableCollection<LogMessage> Logs { get; }
        
        void Log(LogLevel level, string message, string? payload = null);
        void Clear();
    }

    [DataContract]
    public class LogMessage
    {
        [DataMember] public DateTime Timestamp { get; set; } = DateTime.Now;
        [DataMember] public LogLevel Level { get; set; }
        [DataMember] public string Message { get; set; } = string.Empty;
        [DataMember] public string? Payload { get; set; }
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Debug
    }
}
