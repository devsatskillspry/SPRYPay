using System;
using System.Collections.Generic;
using System.Linq;
using SPRYPayServer.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace SPRYPayServer.Services.Wallets
{
    public class SPRYPayWalletProvider
    {
        public WalletRepository WalletRepository { get; }
        public Logs Logs { get; }

        private readonly ExplorerClientProvider _Client;
        readonly SPRYPayNetworkProvider _NetworkProvider;
        readonly IOptions<MemoryCacheOptions> _Options;
        public SPRYPayWalletProvider(ExplorerClientProvider client,
                                    IOptions<MemoryCacheOptions> memoryCacheOption,
                                    Data.ApplicationDbContextFactory dbContextFactory,
                                    SPRYPayNetworkProvider networkProvider,
                                    NBXplorerConnectionFactory nbxplorerConnectionFactory,
                                    WalletRepository walletRepository,
                                    Logs logs)
        {
            ArgumentNullException.ThrowIfNull(client);
            this.Logs = logs;
            _Client = client;
            _NetworkProvider = networkProvider;
            WalletRepository = walletRepository;
            _Options = memoryCacheOption;

            foreach (var network in networkProvider.GetAll().OfType<SPRYPayNetwork>())
            {
                var explorerClient = _Client.GetExplorerClient(network.CryptoCode);
                if (explorerClient == null)
                    continue;
                _Wallets.Add(network.CryptoCode.ToUpperInvariant(), new SPRYPayWallet(explorerClient, new MemoryCache(_Options), network, WalletRepository, dbContextFactory, nbxplorerConnectionFactory, Logs));
            }
        }

        readonly Dictionary<string, SPRYPayWallet> _Wallets = new Dictionary<string, SPRYPayWallet>();

        public SPRYPayWallet GetWallet(SPRYPayNetworkBase network)
        {
            ArgumentNullException.ThrowIfNull(network);
            return GetWallet(network.CryptoCode);
        }
        public SPRYPayWallet GetWallet(string cryptoCode)
        {
            ArgumentNullException.ThrowIfNull(cryptoCode);
            _Wallets.TryGetValue(cryptoCode.ToUpperInvariant(), out var result);
            return result;
        }

        public bool IsAvailable(SPRYPayNetworkBase network)
        {
            return _Client.IsAvailable(network);
        }

        public IEnumerable<SPRYPayWallet> GetWallets()
        {
            foreach (var w in _Wallets)
                yield return w.Value;
        }
    }
}
