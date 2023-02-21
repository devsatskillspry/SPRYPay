using SPRYPayServer.Abstractions.Contracts;
using SPRYPayServer.Abstractions.Models;
using SPRYPayServer.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace SPRYPayServer.Plugins.PayButton
{
    public class PointOfSalePlugin : BaseSPRYPayServerPlugin
    {
        public override string Identifier => "SPRYPayServer.Plugins.PointOfSale";
        public override string Name => "Point Of Sale";
        public override string Description => "Readily accept bitcoin without fees or a third-party, directly to your wallet.";

        public override void Execute(IServiceCollection services)
        {
            services.AddSingleton<IUIExtension>(new UIExtension("PointOfSale/NavExtension", "apps-nav"));
            base.Execute(services);
        }
    }
}
