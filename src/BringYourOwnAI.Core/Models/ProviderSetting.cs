using System.Runtime.Serialization;

namespace BringYourOwnAI.Core.Models
{
    [DataContract]
    public class ProviderSetting
    {
        [DataMember]
        public string Id { get; set; } = string.Empty;

        [DataMember]
        public string Name { get; set; } = string.Empty;

        [DataMember]
        public string ProviderType { get; set; } = string.Empty; // "openai", "ollama", "gemini"

        [DataMember]
        public string ApiKey { get; set; } = string.Empty;

        [DataMember]
        public string Endpoint { get; set; } = string.Empty;

        [DataMember]
        public string Model { get; set; } = string.Empty;
    }
}
