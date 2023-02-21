#nullable enable
using System.Linq;
using SPRYPayServer.Data;

namespace SPRYPayServer
{
    public static class StoreExtensions
    {
        public static DerivationSchemeSettings? GetDerivationSchemeSettings(this StoreData store, SPRYPayNetworkProvider networkProvider, string cryptoCode)
        {
            var paymentMethod = store
                .GetSupportedPaymentMethods(networkProvider)
                .OfType<DerivationSchemeSettings>()
                .FirstOrDefault(p => p.PaymentId.PaymentType == Payments.PaymentTypes.SPRYLike && p.PaymentId.CryptoCode == cryptoCode);
            return paymentMethod;
        }
    }
}
