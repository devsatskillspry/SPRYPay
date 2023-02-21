using System.Threading.Tasks;
using SPRYPayServer.Abstractions.Constants;
using SPRYPayServer.Client;
using SPRYPayServer.Data;
using SPRYPayServer.Security;
using SPRYPayServer.Services.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SPRYPayServer.Controllers.Greenfield
{
    /// <summary>
    /// this controller serves as a testing endpoint for our api key unit tests
    /// </summary>
    [Route("api/test/apikey")]
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
    public class GreenfieldTestApiKeyController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly StoreRepository _storeRepository;
        private readonly SPRYPayServerClient _localSPRYPayServerClient;

        public GreenfieldTestApiKeyController(UserManager<ApplicationUser> userManager, StoreRepository storeRepository, SPRYPayServerClient localSPRYPayServerClient)
        {
            _userManager = userManager;
            _storeRepository = storeRepository;
            _localSPRYPayServerClient = localSPRYPayServerClient;
        }

        [HttpGet("me/id")]
        [Authorize(Policy = Policies.CanViewProfile, AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        public string GetCurrentUserId()
        {
            return _userManager.GetUserId(User);
        }

        [HttpGet("me")]
        [Authorize(Policy = Policies.CanViewProfile, AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        public async Task<ApplicationUser> GetCurrentUser()
        {
            return await _userManager.GetUserAsync(User);
        }

        [HttpGet("me/is-admin")]
        [Authorize(Policy = Policies.CanModifyServerSettings, AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        public bool AmIAnAdmin()
        {
            return true;
        }

        [HttpGet("me/stores")]
        [Authorize(Policy = Policies.CanViewStoreSettings, AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        public StoreData[] GetCurrentUserStores()
        {
            return this.HttpContext.GetStoresData();
        }

        [HttpGet("me/stores/{storeId}/can-view")]
        [Authorize(Policy = Policies.CanViewStoreSettings,
            AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        public bool CanViewStore(string storeId)
        {
            return true;
        }

        [HttpGet("me/stores/{storeId}/can-edit")]
        [Authorize(Policy = Policies.CanModifyStoreSettings,
            AuthenticationSchemes = AuthenticationSchemes.Greenfield)]
        public bool CanEditStore(string storeId)
        {
            return true;
        }
    }
}
