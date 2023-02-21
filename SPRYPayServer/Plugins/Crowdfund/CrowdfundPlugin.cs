using SPRYPayServer.Abstractions.Contracts;
using SPRYPayServer.Abstractions.Models;
using SPRYPayServer.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace SPRYPayServer.Plugins.PayButton
{
    public class CrowdfundPlugin : BaseSPRYPayServerPlugin
    {
        public override string Identifier => "SPRYPayServer.Plugins.Crowdfund";
        public override string Name => "Crowdfund";
        public override string Description => "Create a self-hosted funding campaign, similar to Kickstarter or Indiegogo. Funds go directly to the creator’s wallet without any fees.";

        public override void Execute(IServiceCollection services)
        {
            services.AddSingleton<IUIExtension>(new UIExtension("Crowdfund/NavExtension", "apps-nav"));
            base.Execute(services);
        }
    }
}
