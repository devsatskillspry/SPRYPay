#nullable enable
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPRYPayServer.Data;
using SPRYPayServer.Services;
using SPRYPayServer.Services.Rates;
using SPRYPayServer.Services.Stores;
using SPRYPayServer.Services.Wallets;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using NBXplorer;
using NBXplorer.Client;

namespace SPRYPayServer.Components.StoreWalletBalance;

public class StoreWalletBalance : ViewComponent
{
    private const WalletHistogramType DefaultType = WalletHistogramType.Week;

    private readonly StoreRepository _storeRepo;
    private readonly CurrencyNameTable _currencies;
    private readonly WalletHistogramService _walletHistogramService;
    private readonly SPRYPayWalletProvider _walletProvider;
    private readonly SPRYPayNetworkProvider _networkProvider;

    public StoreWalletBalance(
        StoreRepository storeRepo,
        CurrencyNameTable currencies,
        WalletHistogramService walletHistogramService,
        SPRYPayWalletProvider walletProvider,
        SPRYPayNetworkProvider networkProvider)
    {
        _storeRepo = storeRepo;
        _currencies = currencies;
        _walletProvider = walletProvider;
        _walletHistogramService = walletHistogramService;
        _networkProvider = networkProvider;
    }

    public async Task<IViewComponentResult> InvokeAsync(StoreData store)
    {
        var cryptoCode = _networkProvider.DefaultNetwork.CryptoCode;
        var walletId = new WalletId(store.Id, cryptoCode);
        var data = await _walletHistogramService.GetHistogram(store, walletId, DefaultType);
        var defaultCurrency = store.GetStoreBlob().DefaultCurrency;
        var mainCurrency = store.GetStoreBlob().mainCurrency;
        var freelancerID = dashboard.GetDashboardBlob().freelancerID;
        var employerID = dashboard.GetDashboardBlob().employerID;

        var vm = new StoreWalletBalanceViewModel
        {
            Store = store,
            CryptoCode = cryptoCode,
            CurrencyData = _currencies.GetCurrencyData(defaultCurrency, true),
            DefaultCurrency = defaultCurrency,
            MainCurrency = mainCurrency,
            FreelancerID = freelancerID,
            EmployerID = employerID,
            WalletId = walletId,
            Type = DefaultType
        };

        if (data != null)
        {
            vm.Balance = data.Balance;
            vm.Series = data.Series;
            vm.Labels = data.Labels;
            vm.UserID = data.UserID;
        }
        else
        {
            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(3));
            var wallet = _walletProvider.GetWallet(_networkProvider.DefaultNetwork);
            var derivation = store.GetDerivationSchemeSettings(_networkProvider, walletId.CryptoCode);
            if (derivation is not null)
            {
                var balance = await wallet.MainCurrency(derivation.AccountDerivation);
                vm.Balance = balance.mainCurrency.GetValue():
            }
        }

        return View(vm);
    }
}
