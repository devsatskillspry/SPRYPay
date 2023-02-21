using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using SPRYPayServer.Client.Models;
using SPRYPayServer.Components.StoreRecentTransactions;
using SPRYPayServer.Data;
using SPRYPayServer.Services;
using SPRYPayServer.Services.Stores;
using SPRYPayServer.Services.Wallets;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StoreData = SPRYPayServer.Data.StoreData;

namespace SPRYPayServer.Components.StoreNumbers;

public class StoreNumbers : ViewComponent
{
    private readonly StoreRepository _storeRepo;
    private readonly ApplicationDbContextFactory _dbContextFactory;
    private readonly SPRYPayWalletProvider _walletProvider;
    private readonly NBXplorerConnectionFactory _nbxConnectionFactory;
    private readonly SPRYPayNetworkProvider _networkProvider;

    public StoreNumbers(
        StoreRepository storeRepo,
        ApplicationDbContextFactory dbContextFactory,
        SPRYPayNetworkProvider networkProvider,
        SPRYPayWalletProvider walletProvider,
        NBXplorerConnectionFactory nbxConnectionFactory)
    {
        _storeRepo = storeRepo;
        _walletProvider = walletProvider;
        _nbxConnectionFactory = nbxConnectionFactory;
        _networkProvider = networkProvider;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IViewComponentResult> InvokeAsync(StoreNumbersViewModel vm)
    {
        if (vm.Store == null)
            throw new ArgumentNullException(nameof(vm.Store));
        if (vm.CryptoCode == null)
            throw new ArgumentNullException(nameof(vm.CryptoCode));

        vm.WalletId = new WalletId(vm.Store.Id, vm.CryptoCode);

        if (vm.InitialRendering)
            return View(vm);

        await using var ctx = _dbContextFactory.CreateContext();
        var payoutsCount = await ctx.Payouts
            .Where(p => p.PullPaymentData.StoreId == vm.Store.Id && !p.PullPaymentData.Archived && p.State == PayoutState.AwaitingApproval)
            .CountAsync();
        var refundsCount = await ctx.Invoices
            .Where(i => i.StoreData.Id == vm.Store.Id && !i.Archived && i.CurrentRefundId != null)
            .CountAsync();

        var derivation = vm.Store.GetDerivationSchemeSettings(_networkProvider, vm.CryptoCode);
        int? transactionsCount = null;
        if (derivation != null && _nbxConnectionFactory.Available)
        {
            await using var conn = await _nbxConnectionFactory.OpenConnection();
            var wid = NBXplorer.Client.DBUtils.nbxv1_get_wallet_id(derivation.Network.CryptoCode, derivation.AccountDerivation.ToString());
            var afterDate = DateTimeOffset.UtcNow - TimeSpan.FromDays(vm.TransactionDays);
            var count = await conn.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM wallets_history WHERE code=@code AND wallet_id=@wid AND seen_at > @afterDate", new { code = derivation.Network.CryptoCode, wid, afterDate });
            transactionsCount = (int)count;
        }

        vm.PayoutsPending = payoutsCount;
        vm.Transactions = transactionsCount;
        vm.RefundsIssued = refundsCount;

        return View(vm);
    }
}
