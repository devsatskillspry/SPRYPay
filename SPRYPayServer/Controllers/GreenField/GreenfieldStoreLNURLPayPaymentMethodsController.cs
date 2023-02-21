#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using SPRYPayServer.Abstractions.Constants;
using SPRYPayServer.Abstractions.Contracts;
using SPRYPayServer.Abstractions.Extensions;
using SPRYPayServer.Client;
using SPRYPayServer.Client.Models;
using SPRYPayServer.Configuration;
using SPRYPayServer.Data;
using SPRYPayServer.HostedServices;
using SPRYPayServer.Lightning;
using SPRYPayServer.Payments;
using SPRYPayServer.Payments.Lightning;
using SPRYPayServer.Security;
using SPRYPayServer.Services.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StoreData = SPRYPayServer.Data.StoreData;

namespace SPRYPayServer.Controllers.Greenfield
{
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
    public class GreenfieldStoreLNURLPayPaymentMethodsController : ControllerBase
    {
        private StoreData Store => HttpContext.GetStoreData();
        private readonly StoreRepository _storeRepository;
        private readonly SPRYPayNetworkProvider _btcPayNetworkProvider;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISettingsRepository _settingsRepository;

        public GreenfieldStoreLNURLPayPaymentMethodsController(
            StoreRepository storeRepository,
            SPRYPayNetworkProvider btcPayNetworkProvider,
            IAuthorizationService authorizationService,
            ISettingsRepository settingsRepository)
        {
            _storeRepository = storeRepository;
            _btcPayNetworkProvider = btcPayNetworkProvider;
            _authorizationService = authorizationService;
            _settingsRepository = settingsRepository;
        }

        public static IEnumerable<LNURLPayPaymentMethodData> GetLNURLPayPaymentMethods(StoreData store,
            SPRYPayNetworkProvider networkProvider, bool? enabled)
        {
            var blob = store.GetStoreBlob();
            var excludedPaymentMethods = blob.GetExcludedPaymentMethods();

            return store.GetSupportedPaymentMethods(networkProvider)
                .Where((method) => method.PaymentId.PaymentType == PaymentTypes.LNURLPay)
                .OfType<LNURLPaySupportedPaymentMethod>()
                .Select(paymentMethod =>
                    new LNURLPayPaymentMethodData(
                        paymentMethod.PaymentId.CryptoCode,
                        !excludedPaymentMethods.Match(paymentMethod.PaymentId),
                        paymentMethod.UseBech32Scheme, paymentMethod.EnableForStandardInvoices
                    )
                )
                .Where((result) => enabled is null || enabled == result.Enabled)
                .ToList();
        }

        [Authorize(Policy = Policies.CanModifyStoreSettings, AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpGet("~/api/v1/stores/{storeId}/payment-methods/LNURLPay")]
        public ActionResult<IEnumerable<LNURLPayPaymentMethodData>> GetLNURLPayPaymentMethods(
            string storeId,
            [FromQuery] bool? enabled)
        {
            return Ok(GetLNURLPayPaymentMethods(Store, _btcPayNetworkProvider, enabled));
        }

        [Authorize(Policy = Policies.CanModifyStoreSettings, AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpGet("~/api/v1/stores/{storeId}/payment-methods/LNURLPay/{cryptoCode}")]
        public IActionResult GetLNURLPayPaymentMethod(string storeId, string cryptoCode)
        {
            AssertCryptoCodeWallet(cryptoCode, out _);
            var method = GetExistingLNURLPayPaymentMethod(cryptoCode);
            if (method is null)
            {
                return this.CreateAPIError(404, "paymentmethod-not-found", "The LNURL Payment Method isn't activated");
            }

            return Ok(method);
        }

        [Authorize(Policy = Policies.CanModifyStoreSettings, AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpDelete("~/api/v1/stores/{storeId}/payment-methods/LNURLPay/{cryptoCode}")]
        public async Task<IActionResult> RemoveLNURLPayPaymentMethod(
            string storeId,
            string cryptoCode)
        {

            AssertCryptoCodeWallet(cryptoCode, out _);

            var id = new PaymentMethodId(cryptoCode, PaymentTypes.LNURLPay);
            var store = Store;
            store.SetSupportedPaymentMethod(id, null);
            await _storeRepository.UpdateStore(store);
            return Ok();
        }

        [Authorize(Policy = Policies.CanModifyStoreSettings, AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpPut("~/api/v1/stores/{storeId}/payment-methods/LNURLPay/{cryptoCode}")]
        public async Task<IActionResult> UpdateLNURLPayPaymentMethod(string storeId, string cryptoCode,
            [FromBody] LNURLPayPaymentMethodData paymentMethodData)
        {
            var paymentMethodId = new PaymentMethodId(cryptoCode, PaymentTypes.LNURLPay);

            AssertCryptoCodeWallet(cryptoCode, out _);

            var lnMethod = GreenfieldStoreLightningNetworkPaymentMethodsController.GetExistingLightningLikePaymentMethod(_btcPayNetworkProvider,
                cryptoCode, Store);

            if ((lnMethod is null || lnMethod.Enabled is false) && paymentMethodData.Enabled)
            {
                ModelState.AddModelError(nameof(LNURLPayPaymentMethodData.Enabled),
                    "LNURL Pay cannot be enabled unless the lightning payment method is configured and enabled on this store");
            }

            if (!ModelState.IsValid)
                return this.CreateValidationError(ModelState);

            LNURLPaySupportedPaymentMethod? paymentMethod = new LNURLPaySupportedPaymentMethod()
            {
                CryptoCode = cryptoCode,
                UseBech32Scheme = paymentMethodData.UseBech32Scheme,
                EnableForStandardInvoices = paymentMethodData.EnableForStandardInvoices
            };

            var store = Store;
            var storeBlob = store.GetStoreBlob();
            store.SetSupportedPaymentMethod(paymentMethodId, paymentMethod);
            storeBlob.SetExcluded(paymentMethodId, !paymentMethodData.Enabled);
            store.SetStoreBlob(storeBlob);
            await _storeRepository.UpdateStore(store);
            return Ok(GetExistingLNURLPayPaymentMethod(cryptoCode, store));
        }

        private LNURLPayPaymentMethodData? GetExistingLNURLPayPaymentMethod(string cryptoCode,
            StoreData? store = null)
        {
            store ??= Store;
            var storeBlob = store.GetStoreBlob();
            var id = new PaymentMethodId(cryptoCode, PaymentTypes.LNURLPay);
            var paymentMethod = store
                .GetSupportedPaymentMethods(_btcPayNetworkProvider)
                .OfType<LNURLPaySupportedPaymentMethod>()
                .FirstOrDefault(method => method.PaymentId == id);

            var excluded = storeBlob.IsExcluded(id);
            return paymentMethod is null
                ? null
                : new LNURLPayPaymentMethodData(
                    paymentMethod.PaymentId.CryptoCode,
                    !excluded,
                    paymentMethod.UseBech32Scheme, paymentMethod.EnableForStandardInvoices
                );
        }
        private void AssertCryptoCodeWallet(string cryptoCode, out SPRYPayNetwork network)
        {
            network = _btcPayNetworkProvider.GetNetwork<SPRYPayNetwork>(cryptoCode);
            if (network is null)
                throw new JsonHttpException(this.CreateAPIError(404, "unknown-cryptocode", "This crypto code isn't set up in this SPRYPay Server instance"));
        }

    }
}
