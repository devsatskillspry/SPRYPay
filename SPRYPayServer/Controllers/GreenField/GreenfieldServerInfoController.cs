using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SPRYPayServer.Abstractions.Constants;
using SPRYPayServer.Abstractions.Contracts;
using SPRYPayServer.Client.Models;
using SPRYPayServer.Services;
using SPRYPayServer.Services.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace SPRYPayServer.Controllers.Greenfield
{
    [ApiController]
    [EnableCors(CorsPolicies.All)]
    public class GreenfieldServerInfoController : Controller
    {
        private readonly SPRYPayServerEnvironment _env;
        private readonly PaymentMethodHandlerDictionary _paymentMethodHandlerDictionary;
        private readonly IEnumerable<ISyncSummaryProvider> _summaryProviders;

        public GreenfieldServerInfoController(
            SPRYPayServerEnvironment env,
            PaymentMethodHandlerDictionary paymentMethodHandlerDictionary,
            IEnumerable<ISyncSummaryProvider> summaryProviders)
        {
            _env = env;
            _paymentMethodHandlerDictionary = paymentMethodHandlerDictionary;
            _summaryProviders = summaryProviders;
        }

        [Authorize(AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpGet("~/api/v1/server/info")]
        public ActionResult ServerInfo()
        {
            var supportedPaymentMethods = _paymentMethodHandlerDictionary
                .SelectMany(handler => handler.GetSupportedPaymentMethods().Select(id => id.ToString()))
                .Distinct();

            ServerInfoData model = new ServerInfoData2
            {
                FullySynched = _summaryProviders.All(provider => provider.AllAvailable()),
                SyncStatus = _summaryProviders.SelectMany(provider => provider.GetStatuses()),
                Onion = _env.OnionUrl,
                Version = _env.Version,
                SupportedPaymentMethods = supportedPaymentMethods
            };
            return Ok(model);
        }

        public class ServerInfoData2 : ServerInfoData
        {
            public new IEnumerable<ISyncStatus> SyncStatus { get; set; }
        }
    }
}
