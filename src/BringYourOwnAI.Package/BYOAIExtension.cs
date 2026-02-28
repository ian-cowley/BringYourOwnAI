using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;
using BringYourOwnAI.Core.Services;
using BringYourOwnAI.Core.Interfaces;
using BringYourOwnAI.Providers.Common;
using System.Net.Http;

namespace BringYourOwnAI.Package
{
    /// <summary>
    /// Extension entry point for the VisualStudio.Extensibility extension.
    /// </summary>
    [VisualStudioContribution]
    public class BYOAIExtension : Extension
    {
        /// <inheritdoc />
        public override ExtensionConfiguration ExtensionConfiguration => new()
        {
            Metadata = new(
                id: "BringYourOwnAIExtension.12345678-abcd-1234-abcd-1234567890ab",
                version: this.ExtensionAssemblyVersion,
                publisherName: "BringYourOwnAI",
                displayName: "Bring Your Own AI",
                description: "An AI coding assistant extension.")
        };

        /// <inheritdoc />
        protected override void InitializeServices(IServiceCollection serviceCollection)
        {
            base.InitializeServices(serviceCollection);

            // Infrastructure
            serviceCollection.AddSingleton<HttpClient>();

            // Core Services
            serviceCollection.AddSingleton<ILogService, BringYourOwnAI.Core.Services.InMemoryLogService>();
            serviceCollection.AddSingleton<ISettingsService, LocalSettingsService>();
            serviceCollection.AddSingleton<IMemoryService, MemoryService>();
            serviceCollection.AddSingleton<IConversationService, ConversationService>();
            serviceCollection.AddSingleton<IProviderFactory, ProviderFactory>();

            serviceCollection.AddSingleton<IAgentOrchestrator, AgentOrchestrator>();
            serviceCollection.AddSingleton<IVsSolutionService, BringYourOwnAI.Package.Services.ExtensibilitySolutionService>();

            // ViewModels for Remote UI
            serviceCollection.AddTransient<BringYourOwnAI.UI.ViewModels.ChatViewModel>();
            serviceCollection.AddTransient<BringYourOwnAI.UI.ViewModels.ConversationsViewModel>();
            serviceCollection.AddTransient<BringYourOwnAI.UI.ViewModels.SettingsViewModel>();
            serviceCollection.AddTransient<BringYourOwnAI.UI.ViewModels.MemoryViewModel>();
            serviceCollection.AddTransient<BringYourOwnAI.UI.ViewModels.LogsViewModel>();
            serviceCollection.AddTransient<BringYourOwnAI.UI.ViewModels.MainViewModel>();
        }
    }
}
