using Microsoft.VisualStudio.Extensibility.UI;

namespace BringYourOwnAI.UI.Views
{
    /// <summary>
    /// Serves as the C# RemoteUserControl representation.
    /// The actual UI is defined in ChatWindowControl.xaml which is embedded as a resource
    /// and dynamically hydrated inside the IDE process.
    /// </summary>
    public class ChatWindowControl : RemoteUserControl
    {
        public ChatWindowControl(object dataContext)
            : base(dataContext)
        {
            this.ResourceDictionaries.AddEmbeddedResource("BringYourOwnAI.UI.Styles.Styles.xaml");
        }
    }
}
