using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;
using BringYourOwnAI.UI.Views;
using BringYourOwnAI.UI.ViewModels;

namespace BringYourOwnAI.Package
{
    /// <summary>
    /// Represents the Tool Window hosted in Visual Studio leveraging the Remote UI infrastructure.
    /// </summary>
    [VisualStudioContribution]
    public class ChatToolWindow : ToolWindow
    {
        private readonly MainViewModel _viewModel;

        // Constructor injects MainViewModel defined via Extensibility DI
        public ChatToolWindow(MainViewModel viewModel)
        {
            this.Title = "v2 Chat Client";
            _viewModel = viewModel;
        }

        /// <inheritdoc />
        public override ToolWindowConfiguration ToolWindowConfiguration => new()
        {
            Placement = ToolWindowPlacement.Floating
        };

        /// <inheritdoc />
        public override Task<IRemoteUserControl> GetContentAsync(CancellationToken cancellationToken)
        {
            // Returns the Remote UI component which will stream XAML and DataTemplate properties across process boundaries
            return Task.FromResult<IRemoteUserControl>(new ChatWindowControl(_viewModel));
        }
    }
}
