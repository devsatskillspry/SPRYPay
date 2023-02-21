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
using SPRYPayServer.Services;
using SPRYPayServer.Services.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StoreData = SPRYPayServer.Data.StoreData;

namespace SPRYPayServer.Controllers.Greenfield
{
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
    public class GreenfieldStoreLightningNetworkPaymentMethodsController : ControllerBase
    {
        private StoreData Store => HttpContext.GetStoreData();

        public PoliciesSettings PoliciesSettings { get; }

        private readonly StoreRepository _storeRepository;
        private readonly SPRYPayNetworkProvider _btcPayNetworkProvider;
        private readonly IAuthorizationService _authorizationService;

        public GreenfieldStoreLightningNetworkPaymentMethodsController(
            StoreRepository storeRepository,
            SPRYPayNetworkProvider btcPayNetworkProvider,
            IAuthorizationService authorizationService,
            ISettingsRepository settingsRepository,
            PoliciesSettings policiesSettings)
        {
            _storeRepository = storeRepository;
            _btcPayNetworkProvider = btcPayNetworkProvider;
            _authorizationService = authorizationService;
            PoliciesSettings = policiesSettings;
        }

        public static IEnumerable<LightningNetworkPaymentMethodData> GetLightningPaymentMethods(StoreData store,
            SPRYPayNetworkProvider networkProvider, bool? enabled)
        {
            var blob = store.GetStoreBlob();
            var excludedPaymentMethods = blob.GetExcludedPaymentMethods();

            return store.GetSupportedPaymentMethods(networkProvider)
                .Where((method) => method.PaymentId.PaymentType == PaymentTypes.LightningLike)
                .OfType<LightningSupportedPaymentMethod>()
                .Select(paymentMethod =>
                    new LightningNetworkPaymentMethodData(
                        paymentMethod.PaymentId.CryptoCode,
                        paymentMethod.GetExternalLightningUrl()?.ToString() ??
                        paymentMethod.GetDisplayableConnectionString(),
                        !excludedPaymentMethods.Match(paymentMethod.PaymentId),
                        paymentMethod.PaymentId.ToStringNormalized(),
                        paymentMethod.DisableBOLT11PaymentOption
                    )
                )
                .Where((result) => enabled is null || enabled == result.Enabled)
                .ToList();
        }

        [Authorize(Policy = Policies.CanModifyStoreSettings, AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpGet("~/api/v1/stores/{storeId}/payment-methods/LightningNetwork")]
        public ActionResult<IEnumerable<LightningNetworkPaymentMethodData>> GetLightningPaymentMethods(
            string storeId,
            [FromQuery] bool? enabled)
        {
            return Ok(GetLightningPaymentMethods(Store, _btcPayNetworkProvider, enabled));
        }

        [Authorize(Policy = Policies.CanModifyStoreSettings, AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpGet("~/api/v1/stores/{storeId}/payment-methods/LightningNetwork/{cryptoCode}")]
        public ActionResult<LightningNetworkPaymentMethodData> GetLightningNetworkPaymentMethod(string storeId, string cryptoCode)
        {
            AssertSupportLightning(cryptoCode);

            var method = GetExistingLightningLikePaymentMethod(_btcPayNetworkProvider, cryptoCode, Store);
            if (method is null)
            {
                throw ErrorPaymentMethodNotConfigured();
            }

            return Ok(method);
        }

        protected JsonHttpException ErrorPaymentMethodNotConfigured()
        {
            return new JsonHttpException(this.CreateAPIError(404, "paymentmethod-not-configured", "The lightning payment method is not set up"));
        }

        [Authorize(Policy = Policies.CanModifyStoreSettings, AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpDelete("~/api/v1/stores/{storeId}/payment-methods/LightningNetwork/{cryptoCode}")]
        public async Task<IActionResult> RemoveLightningNetworkPaymentMethod(
            string storeId,
            string cryptoCode)
        {
            AssertSupportLightning(cryptoCode);

            var id = new PaymentMethodId(cryptoCode, PaymentTypes.LightningLike);
            var store = Store;
            store.SetSupportedPaymentMethod(id, null);
            await _storeRepository.UpdateStore(store);
            return Ok();
        }

        [Authorize(Policy = Policies.CanModifyStoreSettings, AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpPut("~/api/v1/stores/{storeId}/payment-methods/LightningNetwork/{cryptoCode}")]
        public async Task<IActionResult> UpdateLightningNetworkPaymentMethod(string storeId, string cryptoCode,
            [FromBody] UpdateLightningNetworkPaymentMethodRequest request)
        {
            var paymentMethodId = new PaymentMethodId(cryptoCode, PaymentTypes.LightningLike);
            AssertSupportLightning(cryptoCode);

            if (string.IsNullOrEmpty(request.ConnectionString))
            {
                ModelState.AddModelError(nameof(LightningNetworkPaymentMethodData.ConnectionString),
                    "Missing connectionString");
            }

            if (!ModelState.IsValid)
                return this.CreateValidationError(ModelState);

            LightningSupportedPaymentMethod? paymentMethod = null;
            var store = Store;
            var storeBlob = store.GetStoreBlob();
            var existing = GetExistingLightningLikePaymentMethod(_btcPayNetworkProvider, cryptoCode, store);
            if (existing == null || existing.ConnectionString != request.ConnectionString)
            {
                if (request.ConnectionString == LightningSupportedPaymentMethod.InternalNode)
                {
                    if (!await CanUseInternalLightning())
                    {
                        return this.CreateAPIPermissionError(Policies.CanUseInternalLightningNode, $"You are not authorized to use the internal lightning node. Either add '{Policies.CanUseInternalLightningNode}' to an API Key, or allow non-admin users to use the internal lightning node in the server settings.");
                    }

                    paymentMethod = new Payments.Lightning.LightningSupportedPaymentMethod()
                    {
                        CryptoCode = paymentMethodId.CryptoCode
                    };
                    paymentMethod.SetInternalNode();
                }
                else
                {
                    if (!LightningConnectionString.TryParse(request.ConnectionString, false,
                        out var connectionString, out var error))
                    {
                        ModelState.AddModelError(nameof(request.ConnectionString), $"Invalid URL ({error})");
                        return this.CreateValidationError(ModelState);
                    }

                    if (connectionString.ConnectionType == LightningConnectionType.LndGRPC)
                    {
                        ModelState.AddModelError(nameof(request.ConnectionString),
                            $"SPRYPay does not support gRPC connections");
                        return this.CreateValidationError(ModelState);
                    }

                    if (!await CanManageServer() && !connectionString.IsSafe())
                    {
                        ModelState.AddModelError(nameof(request.ConnectionString),
                            $"You do not have 'sprypay.server.canmodifyserversettings' rights, so the connection string should not contain 'cookiefilepath', 'macaroondirectorypath', 'macaroonfilepath', and should not point to a local ip or to a dns name ending with '.internal', '.local', '.lan' or '.'.");
                        return this.CreateValidationError(ModelState);
                    }

                    paymentMethod = new Payments.Lightning.LightningSupportedPaymentMethod()
                    {
                        CryptoCode = paymentMethodId.CryptoCode
                    };
                    paymentMethod.SetLightningUrl(connectionString);
                }
            }
            store.SetSupportedPaymentMethod(paymentMethodId, paymentMethod);
            storeBlob.SetExcluded(paymentMethodId, !request.Enabled);
            store.SetStoreBlob(storeBlob);
            await _storeRepository.UpdateStore(store);
            return Ok(GetExistingLightningLikePaymentMethod(_btcPayNetworkProvider, cryptoCode, store));
        }

        public static LightningNetworkPaymentMethodData? GetExistingLightningLikePaymentMethod(SPRYPayNetworkProvider btcPayNetworkProvider, string cryptoCode,
            StoreData store)
        {

            var storeBlob = store.GetStoreBlob();
            var id = new PaymentMethodId(cryptoCode, PaymentTypes.LightningLike);
            var paymentMethod = store
                .GetSupportedPaymentMethods(btcPayNetworkProvider)
                .OfType<LightningSupportedPaymentMethod>()
                .FirstOrDefault(method => method.PaymentId == id);

            var excluded = storeBlob.IsExcluded(id);
            return paymentMethod is null
                ? null
                : new LightningNetworkPaymentMethodData(paymentMethod.PaymentId.CryptoCode,
                    paymentMethod.GetDisplayableConnectionString(), !excluded,
                    paymentMethod.PaymentId.ToStringNormalized(), paymentMethod.DisableBOLT11PaymentOption);
        }

        private SPRYPayNetwork AssertSupportLightning(string cryptoCode)
        {
            var network = _btcPayNetworkProvider.GetNetwork<SPRYPayNetwork>(cryptoCode);
            if (network is null)
                throw new JsonHttpException(this.CreateAPIError(404, "unknown-cryptocode", "This crypto code isn't set up in this SPRYPay Server instance"));
            if (!(network.SupportLightning is true))
                throw new JsonHttpException(this.CreateAPIError(404, "unknown-cryptocode", "This crypto code doesn't support lightning"));
            return network;
        }

        private async Task<bool> CanUseInternalLightning()
        {
            return PoliciesSettings.AllowLightningInternalNodeForAll ||
                   (await _authorizationService.AuthorizeAsync(User, null,
                       new PolicyRequirement(Policies.CanUseInternalLightningNode))).Succeeded;
        }

        private async Task<bool> CanManageServer()
        {
            return
                (await _authorizationService.AuthorizeAsync(User, null,
                    new PolicyRequirement(Policies.CanModifyServerSettings))).Succeeded;
        }
    }
}
