using System.Collections.Generic;
using System.Linq;
using SPRYPayServer.Abstractions.Constants;
using SPRYPayServer.Abstractions.Custodians;
using SPRYPayServer.Client.Models;
using SPRYPayServer.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace SPRYPayServer.Controllers.Greenfield
{
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.GreenfieldAPIKeys)]
    [EnableCors(CorsPolicies.All)]
    [ExperimentalRouteAttribute] // if you remove this, also remove "x_experimental": true in swagger.template.custodians.json
    public class GreenfieldCustodianController : ControllerBase
    {
        private readonly IEnumerable<ICustodian> _custodianRegistry;

        public GreenfieldCustodianController(IEnumerable<ICustodian> custodianRegistry)
        {
            _custodianRegistry = custodianRegistry;
        }

        [HttpGet("~/api/v1/custodians")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        public IActionResult ListCustodians()
        {
            var all = _custodianRegistry.ToList().Select(ToModel);
            return Ok(all);
        }

        private CustodianData ToModel(ICustodian custodian)
        {
            var result = new CustodianData();
            result.Code = custodian.Code;
            result.Name = custodian.Name;

            if (custodian is ICanTrade tradableCustodian)
            {
                var tradableAssetPairs = tradableCustodian.GetTradableAssetPairs();
                var tradableAssetPairsDict = new Dictionary<string, AssetPairData>(tradableAssetPairs.Count);
                foreach (var tradableAssetPair in tradableAssetPairs)
                {
                    tradableAssetPairsDict.Add(tradableAssetPair.ToString(), tradableAssetPair);
                }
                result.TradableAssetPairs = tradableAssetPairsDict;
            }

            if (custodian is ICanDeposit depositableCustodian)
            {
                result.DepositablePaymentMethods = depositableCustodian.GetDepositablePaymentMethods();
            }
            if (custodian is ICanWithdraw withdrawableCustodian)
            {
                result.WithdrawablePaymentMethods = withdrawableCustodian.GetWithdrawablePaymentMethods();
            }
            return result;
        }

    }
}
