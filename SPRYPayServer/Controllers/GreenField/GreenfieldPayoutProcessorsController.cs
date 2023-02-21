#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SPRYPayServer.Abstractions.Constants;
using SPRYPayServer.Client;
using SPRYPayServer.Client.Models;
using SPRYPayServer.Data;
using SPRYPayServer.PayoutProcessors;
using SPRYPayServer.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreData = SPRYPayServer.Data.StoreData;

namespace SPRYPayServer.Controllers.Greenfield
{
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
    public class GreenfieldPayoutProcessorsController : ControllerBase
    {
        private readonly IEnumerable<IPayoutProcessorFactory> _factories;

        public GreenfieldPayoutProcessorsController(IEnumerable<IPayoutProcessorFactory> factories)
        {
            _factories = factories;
        }

        [Authorize(AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpGet("~/api/v1/payout-processors")]
        public IActionResult GetPayoutProcessors()
        {
            return Ok(_factories.Select(factory => new PayoutProcessorData()
            {
                Name = factory.Processor,
                FriendlyName = factory.FriendlyName,
                PaymentMethods = factory.GetSupportedPaymentMethods().Select(id => id.ToStringNormalized())
                    .ToArray()
            }));
        }
    }


}
