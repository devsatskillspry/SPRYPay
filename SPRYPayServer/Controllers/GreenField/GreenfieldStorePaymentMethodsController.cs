#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SPRYPayServer.Abstractions.Constants;
using SPRYPayServer.Client;
using SPRYPayServer.Client.Models;
using SPRYPayServer.Data;
using SPRYPayServer.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreData = SPRYPayServer.Data.StoreData;

namespace SPRYPayServer.Controllers.Greenfield
{
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
    public class GreenfieldStorePaymentMethodsController : ControllerBase
    {
        private StoreData Store => HttpContext.GetStoreData();
        private readonly SPRYPayNetworkProvider _btcPayNetworkProvider;
        private readonly IAuthorizationService _authorizationService;

        public GreenfieldStorePaymentMethodsController(SPRYPayNetworkProvider btcPayNetworkProvider, IAuthorizationService authorizationService)
        {
            _btcPayNetworkProvider = btcPayNetworkProvider;
            _authorizationService = authorizationService;
        }

        [Authorize(Policy = Policies.CanViewStoreSettings, AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpGet("~/api/v1/stores/{storeId}/payment-methods")]
        public async Task<ActionResult<Dictionary<string, GenericPaymentMethodData>>> GetStorePaymentMethods(
            string storeId,
            [FromQuery] bool? enabled)
        {
            var storeBlob = Store.GetStoreBlob();
            var excludedPaymentMethods = storeBlob.GetExcludedPaymentMethods();
            var canModifyStore = (await _authorizationService.AuthorizeAsync(User, null,
                new PolicyRequirement(Policies.CanModifyStoreSettings))).Succeeded;
            ;
            return Ok(Store.GetSupportedPaymentMethods(_btcPayNetworkProvider)
                .Where(method =>
                    enabled is null || (enabled is false && excludedPaymentMethods.Match(method.PaymentId)))
                .ToDictionary(
                    method => method.PaymentId.ToStringNormalized(),
                    method => new GenericPaymentMethodData()
                    {
                        CryptoCode = method.PaymentId.CryptoCode,
                        Enabled = enabled.GetValueOrDefault(!excludedPaymentMethods.Match(method.PaymentId)),
                        Data = method.PaymentId.PaymentType.GetGreenfieldData(method, canModifyStore)
                    }));
        }
    }
}
