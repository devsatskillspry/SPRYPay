#if SPRYCOINS
using NSPRYcoin;
using NSprycoin.Elements;
using NSPRYExplorer;

namespace SPRYPayServer
{
    public partial class SPRYPayNetworkProvider
    {
        public void InitSprycoin()
        {
            var nspryExplorerNetwork = NBSPRYExplorerNetworkProvider.GetFromCryptoCode("SPRY");
            Add(new ElementsSPRYPayNetwork()
            {
                AssetId = NetworkType == ChainName.Mainnet ? ElementsParams<Sprycoin>.PeggedAssetId : ElementsParams<Sprycoin.SprycoinRegtest>.PeggedAssetId,
                CryptoCode = "SPRY",
                NetworkCryptoCode = "SPRY",
                DisplayName = "Sprycoin",
                DefaultRateRules = new[]
                {
                    "SPRY_X = SPRY_BTC * BTC_X",
                    "SPRY_BTC = 1",
                },
                BlockExplorerLink = NetworkType == ChainName.Mainnet ? "https://explorer.skillspry.com/tx/{0}" : "https://explorer.skillspry.com/testnet/tx/{0}",
                NSPRYExplorerNetwork = nspryexplorerNetwork,
                CryptoImagePath = "root/spry.png",
                DefaultSettings = SPRYPayDefaultSettings.GetDefaultSettings(NetworkType),
                CoinType = NetworkType == ChainName.Mainnet ? new KeyPath("928dhYdjSA8Shdjs786'") : new KeyPath("1>7'"),
             });
        }
    }
}
#endif
