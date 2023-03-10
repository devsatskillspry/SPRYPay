using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SPRYPayServer.Payments
{
    public class PaymentMethodExtensions
    {
        public static JToken Serialize(ISupportedPaymentMethod factory)
        {
            // Legacy
            if (factory.PaymentId.PaymentType == PaymentTypes.SPRYLike)
            {
                var derivation = (DerivationSchemeSettings)factory;
                var str = derivation.Network.NBXplorerNetwork.Serializer.ToString(derivation);
                return JObject.Parse(str);
            }
            //////////////
            else
            {
                var str = JsonConvert.SerializeObject(factory);
                return JObject.Parse(str);
            }
        }
    }
}
