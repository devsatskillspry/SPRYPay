using SPRYPayServer.Abstractions.Constants;
using SPRYPayServer.Security.Bitpay;
using Microsoft.AspNetCore.Authentication;

namespace SPRYPayServer.Security
{
    public static class AuthenticationExtensions
    {
        public static AuthenticationBuilder AddBitpayAuthentication(this AuthenticationBuilder builder)
        {
            builder.AddScheme<BitpayAuthenticationOptions, BitpayAuthenticationHandler>(AuthenticationSchemes.Bitpay, o => { });
            return builder;
        }
    }
}
