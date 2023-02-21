#nullable enable
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SPRYPayServer.Abstractions.Extensions;
using SPRYPayServer.Client;
using SPRYPayServer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthenticationSchemes = SPRYPayServer.Abstractions.Constants.AuthenticationSchemes;
using LightningAddressData = SPRYPayServer.Client.Models.LightningAddressData;

namespace SPRYPayServer.Controllers.Greenfield
{
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
    public class GreenfieldStoreLightningAddressesController : ControllerBase
    {
        private readonly LightningAddressService _lightningAddressService;

        public GreenfieldStoreLightningAddressesController(
            LightningAddressService lightningAddressService)
        {
            _lightningAddressService = lightningAddressService;
        }

        private LightningAddressData ToModel(SPRYPayServer.Data.LightningAddressData data)
        {
            var blob = data.Blob.GetBlob<LightningAddressDataBlob>();
            return new LightningAddressData()
            {
                Username = data.Username, Max = blob.Max, Min = blob.Min, CurrencyCode = blob.CurrencyCode
            };
        }

        [Authorize(Policy = Policies.CanViewStoreSettings, AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpGet("~/api/v1/stores/{storeId}/lightning-addresses")]
        public async Task<IActionResult> GetStoreLightningAddresses(string storeId)
        {
            return Ok((await _lightningAddressService.Get(new LightningAddressQuery() {StoreIds = new[] {storeId}}))
                .Select(ToModel).ToArray());
        }

        [Authorize(Policy = Policies.CanModifyStoreSettings, AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpDelete("~/api/v1/stores/{storeId}/lightning-addresses/{username}")]
        public async Task<IActionResult> RemoveStoreLightningAddress(string storeId, string username)
        {
            if (await _lightningAddressService.Remove(username, storeId))
            {
                return Ok();
            }
            return
                this.CreateAPIError(404, "lightning-address-not-found", "The lightning address was not present.");

        }

        [Authorize(Policy = Policies.CanViewStoreSettings, AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpGet("~/api/v1/stores/{storeId}/lightning-addresses/{username}")]
        public async Task<IActionResult> GetStoreLightningAddress(string storeId, string username)
        {
            var res = await _lightningAddressService.Get(new LightningAddressQuery()
            {
                Usernames = new[] {username}, StoreIds = new[] {storeId},
            });
            return res?.Any() is true ? Ok(ToModel(res.First())) : this.CreateAPIError(404, "lightning-address-not-found", "The lightning address was not present.");
        }

        [Authorize(Policy = Policies.CanModifyStoreSettings, AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        [HttpPost("~/api/v1/stores/{storeId}/lightning-addresses/{username}")]
        public async Task<IActionResult> AddOrUpdateStoreLightningAddress(
            string storeId, string username, LightningAddressData data)
        {
            if (data.Min <= 0)
            {
                ModelState.AddModelError(nameof(data.Min), "Minimum must be greater than 0 if provided.");
                return this.CreateValidationError(ModelState);
            }
            
            if (await _lightningAddressService.Set(new Data.LightningAddressData()
                {
                    StoreDataId = storeId,
                    Username = username,
                    Blob = new LightningAddressDataBlob()
                    {
                        Max = data.Max, Min = data.Min, CurrencyCode = data.CurrencyCode
                    }.SerializeBlob()
                }))
            {
                return await GetStoreLightningAddress(storeId, username);
            }

            return this.CreateAPIError((int)HttpStatusCode.BadRequest, "username-already-used",
                "The username is already in use by another store.");
        }
    }
}
