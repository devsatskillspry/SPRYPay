using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPRYPayServer.Abstractions.Contracts;
using SPRYPayServer.Services;

namespace SPRYPayServer.Hosting
{
    public class BlockExplorerLinkStartupTask : IStartupTask
    {
        private readonly SettingsRepository _settingsRepository;
        private readonly SPRYPayNetworkProvider _btcPayNetworkProvider;

        public BlockExplorerLinkStartupTask(SettingsRepository settingsRepository,
            SPRYPayNetworkProvider btcPayNetworkProvider)
        {
            _settingsRepository = settingsRepository;
            _btcPayNetworkProvider = btcPayNetworkProvider;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var settings = await _settingsRepository.GetSettingAsync<PoliciesSettings>();
            if (settings?.BlockExplorerLinks?.Any() is true)
            {
                SetLinkOnNetworks(settings.BlockExplorerLinks, _btcPayNetworkProvider);
            }
        }

        public static void SetLinkOnNetworks(List<PoliciesSettings.BlockExplorerOverrideItem> links,
            SPRYPayNetworkProvider networkProvider)
        {
            var networks = networkProvider.GetAll();
            foreach (var network in networks)
            {
                var overrideLink = links.SingleOrDefault(item =>
                    item.CryptoCode.Equals(network.CryptoCode, StringComparison.InvariantCultureIgnoreCase));
                network.BlockExplorerLink = overrideLink?.Link ?? network.BlockExplorerLinkDefault;

            }
        }
    }
}
