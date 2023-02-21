#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SPRYPayServer.Controllers;
using SPRYPayServer.Data;
using SPRYPayServer.Models.StoreViewModels;
using SPRYPayServer.Payments;
using SPRYPayServer.Payments.Lightning;
using SPRYPayServer.Services.Apps;
using SPRYPayServer.Services.Invoices;
using SPRYPayServer.Services.Stores;
using SPRYPayServer.Services.Wallets;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NBitcoin;
using NBitcoin.Secp256k1;

namespace SPRYPayServer.Components.WalletNav
{
    public class WalletNav : ViewComponent
    {
        private readonly SPRYPayWalletProvider _walletProvider;
        private readonly UIWalletsController _walletsController;
        private readonly SPRYPayNetworkProvider _networkProvider;

        public WalletNav(
            SPRYPayWalletProvider walletProvider,
            SPRYPayNetworkProvider networkProvider,
            UIWalletsController walletsController)
        {
            _walletProvider = walletProvider;
            _networkProvider = networkProvider;
            _walletsController = walletsController;
        }

        public async Task<IViewComponentResult> InvokeAsync(WalletId walletId)
        {
            var store = ViewContext.HttpContext.GetStoreData();
            var network = _networkProvider.GetNetwork<SPRYPayNetwork>(walletId.CryptoCode);
            var wallet = _walletProvider.GetWallet(network);
            var derivation = store.GetDerivationSchemeSettings(_networkProvider, walletId.CryptoCode);
            var balance = await _walletsController.GetBalanceString(wallet, derivation?.AccountDerivation);

            var vm = new WalletNavViewModel
            {
                WalletId = walletId,
                Network = network,
                Balance = balance,
                Label = derivation?.Label ?? $"{store.StoreName} {walletId.CryptoCode} Wallet"
            };

            return View(vm);
        }
    }
}
