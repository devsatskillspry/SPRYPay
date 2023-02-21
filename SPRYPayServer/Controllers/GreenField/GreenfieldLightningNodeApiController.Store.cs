using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPRYPayServer.Abstractions.Constants;
using SPRYPayServer.Abstractions.Contracts;
using SPRYPayServer.Abstractions.Extensions;
using SPRYPayServer.Client;
using SPRYPayServer.Client.Models;
using SPRYPayServer.Configuration;
using SPRYPayServer.Data;
using SPRYPayServer.Lightning;
using SPRYPayServer.Payments;
using SPRYPayServer.Payments.Lightning;
using SPRYPayServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SPRYPayServer.Controllers.Greenfield
{
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
    [LightningUnavailableExceptionFilter]
    [EnableCors(CorsPolicies.All)]
    public class GreenfieldStoreLightningNodeApiController : GreenfieldLightningNodeApiController
    {
        private readonly IOptions<LightningNetworkOptions> _lightningNetworkOptions;
        private readonly LightningClientFactoryService _lightningClientFactory;
        private readonly SPRYPayNetworkProvider _btcPayNetworkProvider;

        public GreenfieldStoreLightningNodeApiController(
            IOptions<LightningNetworkOptions> lightningNetworkOptions,
            LightningClientFactoryService lightningClientFactory, SPRYPayNetworkProvider btcPayNetworkProvider,
            PoliciesSettings policiesSettings,
            IAuthorizationService authorizationService) : base(
            btcPayNetworkProvider, policiesSettings, authorizationService)
        {
            _lightningNetworkOptions = lightningNetworkOptions;
            _lightningClientFactory = lightningClientFactory;
            _btcPayNetworkProvider = btcPayNetworkProvider;
        }

        [Authorize(Policy = Policies.CanUseLightningNodeInStore,
            AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpGet("~/api/v1/stores/{storeId}/lightning/{cryptoCode}/info")]
        public override Task<IActionResult> GetInfo(string cryptoCode, CancellationToken cancellationToken = default)
        {
            return base.GetInfo(cryptoCode, cancellationToken);
        }

        [Authorize(Policy = Policies.CanUseLightningNodeInStore,
            AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpGet("~/api/v1/stores/{storeId}/lightning/{cryptoCode}/balance")]
        public override Task<IActionResult> GetBalance(string cryptoCode, CancellationToken cancellationToken = default)
        {
            return base.GetBalance(cryptoCode, cancellationToken);
        }

        [Authorize(Policy = Policies.CanUseLightningNodeInStore,
            AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpPost("~/api/v1/stores/{storeId}/lightning/{cryptoCode}/connect")]
        public override Task<IActionResult> ConnectToNode(string cryptoCode, ConnectToNodeRequest request, CancellationToken cancellationToken = default)
        {
            return base.ConnectToNode(cryptoCode, request, cancellationToken);
        }
        [Authorize(Policy = Policies.CanUseLightningNodeInStore,
            AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpGet("~/api/v1/stores/{storeId}/lightning/{cryptoCode}/channels")]
        public override Task<IActionResult> GetChannels(string cryptoCode, CancellationToken cancellationToken = default)
        {
            return base.GetChannels(cryptoCode, cancellationToken);
        }
        [Authorize(Policy = Policies.CanUseLightningNodeInStore,
            AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpPost("~/api/v1/stores/{storeId}/lightning/{cryptoCode}/channels")]
        public override Task<IActionResult> OpenChannel(string cryptoCode, OpenLightningChannelRequest request, CancellationToken cancellationToken = default)
        {
            return base.OpenChannel(cryptoCode, request, cancellationToken);
        }

        [Authorize(Policy = Policies.CanUseLightningNodeInStore,
            AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpPost("~/api/v1/stores/{storeId}/lightning/{cryptoCode}/address")]
        public override Task<IActionResult> GetDepositAddress(string cryptoCode, CancellationToken cancellationToken = default)
        {
            return base.GetDepositAddress(cryptoCode, cancellationToken);
        }

        [Authorize(Policy = Policies.CanUseLightningNodeInStore,
            AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpGet("~/api/v1/stores/{storeId}/lightning/{cryptoCode}/payments/{paymentHash}")]
        public override Task<IActionResult> GetPayment(string cryptoCode, string paymentHash, CancellationToken cancellationToken = default)
        {
            return base.GetPayment(cryptoCode, paymentHash, cancellationToken);
        }

        [Authorize(Policy = Policies.CanUseLightningNodeInStore,
            AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpPost("~/api/v1/stores/{storeId}/lightning/{cryptoCode}/invoices/pay")]
        public override Task<IActionResult> PayInvoice(string cryptoCode, PayLightningInvoiceRequest lightningInvoice, CancellationToken cancellationToken = default)
        {
            return base.PayInvoice(cryptoCode, lightningInvoice, cancellationToken);
        }

        [Authorize(Policy = Policies.CanViewLightningInvoiceInStore,
            AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpGet("~/api/v1/stores/{storeId}/lightning/{cryptoCode}/invoices/{id}")]
        public override Task<IActionResult> GetInvoice(string cryptoCode, string id, CancellationToken cancellationToken = default)
        {
            return base.GetInvoice(cryptoCode, id, cancellationToken);
        }

        [Authorize(Policy = Policies.CanViewLightningInvoiceInStore,
            AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpGet("~/api/v1/stores/{storeId}/lightning/{cryptoCode}/invoices")]
        public override Task<IActionResult> GetInvoices(string cryptoCode, [FromQuery] bool? pendingOnly, [FromQuery] long? offsetIndex, CancellationToken cancellationToken = default)
        {
            return base.GetInvoices(cryptoCode, pendingOnly, offsetIndex, cancellationToken);
        }

        [Authorize(Policy = Policies.CanCreateLightningInvoiceInStore,
            AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpPost("~/api/v1/stores/{storeId}/lightning/{cryptoCode}/invoices")]
        public override Task<IActionResult> CreateInvoice(string cryptoCode, CreateLightningInvoiceRequest request, CancellationToken cancellationToken = default)
        {
            return base.CreateInvoice(cryptoCode, request, cancellationToken);
        }

        [Authorize(Policy = Policies.CanUseLightningNodeInStore,
            AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpGet("~/api/v1/stores/{storeId}/lightning/{cryptoCode}/payments")]
        public override Task<IActionResult> GetPayments(string cryptoCode, [FromQuery] bool? includePending, [FromQuery] long? offsetIndex, CancellationToken cancellationToken = default)
        {
            return base.GetPayments(cryptoCode, includePending, offsetIndex, cancellationToken);
        }

        protected override Task<ILightningClient> GetLightningClient(string cryptoCode,
            bool doingAdminThings)
        {
            var network = _btcPayNetworkProvider.GetNetwork<SPRYPayNetwork>(cryptoCode);
            if (network == null)
            {
                throw ErrorCryptoCodeNotFound();
            }

            var store = HttpContext.GetStoreData();
            if (store == null)
            {
                throw new JsonHttpException(StoreNotFound());
            }

            var id = new PaymentMethodId(cryptoCode, PaymentTypes.LightningLike);
            var existing = store.GetSupportedPaymentMethods(_btcPayNetworkProvider)
                .OfType<LightningSupportedPaymentMethod>()
                .FirstOrDefault(d => d.PaymentId == id);
            if (existing == null)
                throw ErrorLightningNodeNotConfiguredForStore();
            if (existing.GetExternalLightningUrl() is LightningConnectionString connectionString)
            {
                return Task.FromResult(_lightningClientFactory.Create(connectionString, network));
            }
            else if (existing.IsInternalNode &&
            _lightningNetworkOptions.Value.InternalLightningByCryptoCode.TryGetValue(network.CryptoCode,
            out var internalLightningNode))
            {
                if (!User.IsInRole(Roles.ServerAdmin) && doingAdminThings)
                {
                    throw ErrorShouldBeAdminForInternalNode();
                }
                return Task.FromResult(_lightningClientFactory.Create(internalLightningNode, network));
            }
            throw ErrorLightningNodeNotConfiguredForStore();
        }

        private IActionResult StoreNotFound()
        {
            return this.CreateAPIError(404, "store-not-found", "The store was not found");
        }
    }
}
