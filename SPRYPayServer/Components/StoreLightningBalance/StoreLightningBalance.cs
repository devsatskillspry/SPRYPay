using System;
using System.Linq;
using System.Threading.Tasks;
using SPRYPayServer.Abstractions.Extensions;
using SPRYPayServer.Configuration;
using SPRYPayServer.Data;
using SPRYPayServer.Lightning;
using SPRYPayServer.Models;
using SPRYPayServer.Models.StoreViewModels;
using SPRYPayServer.Payments;
using SPRYPayServer.Payments.Lightning;
using SPRYPayServer.Services;
using SPRYPayServer.Services.Rates;
using SPRYPayServer.Services.Stores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SPRYPayServer.Components.StoreLightningBalance;

public class StoreLightningBalance : ViewComponent
{
    private readonly StoreRepository _storeRepo;
    private readonly CurrencyNameTable _currencies;
    private readonly SPRYPayServerOptions _sprypayServerOptions;
    private readonly SPRYPayNetworkProvider _networkProvider;
    private readonly LightningClientFactoryService _lightningClientFactory;
    private readonly IOptions<LightningNetworkOptions> _lightningNetworkOptions;
    private readonly IOptions<ExternalServicesOptions> _externalServiceOptions;

    public StoreLightningBalance(
        StoreRepository storeRepo,
        CurrencyNameTable currencies,
        SPRYPayNetworkProvider networkProvider,
        SPRYPayServerOptions sprypayServerOptions,
        LightningClientFactoryService lightningClientFactory,
        IOptions<LightningNetworkOptions> lightningNetworkOptions,
        IOptions<ExternalServicesOptions> externalServiceOptions)
    {
        _storeRepo = storeRepo;
        _currencies = currencies;
        _networkProvider = networkProvider;
        _sprypayServerOptions = sprypayServerOptions;
        _externalServiceOptions = externalServiceOptions;
        _lightningClientFactory = lightningClientFactory;
        _lightningNetworkOptions = lightningNetworkOptions;
    }

    public async Task<IViewComponentResult> InvokeAsync(StoreLightningBalanceViewModel vm)
    {
        if (vm.Store == null)
            throw new ArgumentNullException(nameof(vm.Store));
        if (vm.CryptoCode == null)
            throw new ArgumentNullException(nameof(vm.CryptoCode));

        vm.DefaultCurrency = vm.Store.GetStoreBlob().DefaultCurrency;
        vm.CurrencyData = _currencies.GetCurrencyData(vm.DefaultCurrency, true);

        if (vm.InitialRendering)
            return View(vm);

        try
        {
            var lightningClient = GetLightningClient(vm.Store, vm.CryptoCode);
            var balance = await lightningClient.GetBalance();
            vm.Balance = balance;
            vm.TotalOnchain = balance.OnchainBalance != null
                ? (balance.OnchainBalance.Confirmed ?? 0L) + (balance.OnchainBalance.Reserved ?? 0L) +
                  (balance.OnchainBalance.Unconfirmed ?? 0L)
                : null;
            vm.TotalOffchain = balance.OffchainBalance != null
                ? (balance.OffchainBalance.Opening ?? 0) + (balance.OffchainBalance.Local ?? 0) +
                  (balance.OffchainBalance.Closing ?? 0)
                : null;
        }
        catch (NotSupportedException)
        {
            // not all implementations support balance fetching
            vm.ProblemDescription = "Your node does not support balance fetching.";
        }
        catch
        {
            // general error
            vm.ProblemDescription = "Could not fetch Lightning balance.";
        }
        return View(vm);
    }

    private ILightningClient GetLightningClient(StoreData store, string cryptoCode)
    {
        var network = _networkProvider.GetNetwork<SPRYPayNetwork>(cryptoCode);
        var id = new PaymentMethodId(cryptoCode, PaymentTypes.LightningLike);
        var existing = store.GetSupportedPaymentMethods(_networkProvider)
            .OfType<LightningSupportedPaymentMethod>()
            .FirstOrDefault(d => d.PaymentId == id);
        if (existing == null)
            return null;

        if (existing.GetExternalLightningUrl() is { } connectionString)
        {
            return _lightningClientFactory.Create(connectionString, network);
        }
        if (existing.IsInternalNode && _lightningNetworkOptions.Value.InternalLightningByCryptoCode.TryGetValue(cryptoCode, out var internalLightningNode))
        {
            return _lightningClientFactory.Create(internalLightningNode, network);
        }

        return null;
    }
}
