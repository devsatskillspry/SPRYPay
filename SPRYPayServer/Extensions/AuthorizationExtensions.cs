using System.Security.Claims;
using System.Threading.Tasks;
using SPRYPayServer.Abstractions.Constants;
using SPRYPayServer.Client;
using SPRYPayServer.Security.Bitpay;
using SPRYPayServer.Security.Greenfield;
using SPRYPayServer.Services;
using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Authorization;

namespace SPRYPayServer
{
    public static class AuthorizationExtensions
    {
        public static async Task<(bool HotWallet, bool RPCImport)> CanUseHotWallet(
            this IAuthorizationService authorizationService,
            PoliciesSettings policiesSettings,
            ClaimsPrincipal user)
        {
            if (!user.Identity.IsAuthenticated)
                return (false, false);
            var claimUser = user.Identity as ClaimsIdentity;
            if (claimUser is null)
                return (false, false);

            bool isAdmin = false;
            if (claimUser.AuthenticationType == AuthenticationSchemes.Cookie)
                isAdmin = user.IsInRole(Roles.ServerAdmin);
            else if (claimUser.AuthenticationType == GreenfieldConstants.AuthenticationType)
                isAdmin = (await authorizationService.AuthorizeAsync(user, Policies.CanModifyServerSettings)).Succeeded;
            return isAdmin ? (true, true) :
                   (policiesSettings?.AllowHotWalletForAll is true, policiesSettings?.AllowHotWalletRPCImportForAll is true);
        }
    }
}
