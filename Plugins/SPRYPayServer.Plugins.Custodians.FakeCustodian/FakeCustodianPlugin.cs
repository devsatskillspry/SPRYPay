using SPRYPayServer.Abstractions.Custodians;
using SPRYPayServer.Abstractions.Models;
using Microsoft.Extensions.DependencyInjection;

namespace SPRYPayServer.Plugins.Custodians.FakeCustodian
{
    public class FakeCustodianPlugin : BaseSPRYPayServerPlugin
    {
        public override string Identifier { get; } = "SPRYPayServer.Plugins.Custodians.Fake";
        public override string Name { get; } = "Custodian: Fake";
        public override string Description { get; } = "Adds a fake custodian for testing";

        public override void Execute(IServiceCollection services)
        {
            services.AddSingleton<FakeCustodian>();
            services.AddSingleton<ICustodian, FakeCustodian>(provider => provider.GetRequiredService<FakeCustodian>());
        }
    }
}
