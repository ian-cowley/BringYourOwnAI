using System.Runtime.Serialization;

namespace BringYourOwnAI.Core.Models
{
    [DataContract]
    public class Settings
    {
        [DataMember]
        public System.Collections.Generic.List<ProviderSetting> Providers { get; set; } = new System.Collections.Generic.List<ProviderSetting>();

        [DataMember]
        public bool AutoSelectModels { get; set; } = true;
    }
}
