using SPRYPayServer.Abstractions.Contracts;
using SPRYPayServer.Abstractions.Models;
using SPRYPayServer.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace SPRYPayServer.Plugins.PayButton
{
    public class PayButtonPlugin : BaseSPRYPayServerPlugin
    {
        public override string Identifier => "SPRYPayServer.Plugins.PayButton";
        public override string Name => "Pay Button";
        public override string Description => "Easily-embeddable HTML button for accepting tips and donations .";

        public override void Execute(IServiceCollection services)
        {
            services.AddSingleton<IUIExtension>(new UIExtension("PayButton/NavExtension", "header-nav"));
            base.Execute(services);
        }
    }
}
