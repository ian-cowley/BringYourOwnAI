using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;

namespace BringYourOwnAI.Package
{
    /// <summary>
    /// Command to show the Bring Your Own AI chat window.
    /// </summary>
    [VisualStudioContribution]
    public class ShowChatWindowCommand : Command
    {
        /// <inheritdoc />
        public override CommandConfiguration CommandConfiguration => new("Bring Your Own AI Chat")
        {
            Icon = new(ImageMoniker.KnownValues.Extension, IconSettings.IconAndText),
            Placements = new[] { CommandPlacement.KnownPlacements.ToolsMenu }
        };

        /// <inheritdoc />
        public override Task InitializeAsync(CancellationToken cancellationToken)
        {
            return base.InitializeAsync(cancellationToken);
        }

        /// <inheritdoc />
        public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
        {
            // Shell() API requires Extensibility model
            await this.Extensibility.Shell().ShowToolWindowAsync<ChatToolWindow>(activate: true, cancellationToken: cancellationToken);
        }
    }
}
