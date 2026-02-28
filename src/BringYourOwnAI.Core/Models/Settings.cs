using System.Runtime.Serialization;

namespace BringYourOwnAI.Core.Models
{
    [DataContract]
    public class Settings
    {
        [DataMember]
        public string OpenAiKey { get; set; } = string.Empty;

        [DataMember]
        public string OllamaEndpoint { get; set; } = "http://localhost:11434";

        [DataMember]
        public string GeminiKey { get; set; } = string.Empty;

        [DataMember]
        public bool AutoSelectModels { get; set; } = true;
    }
}
