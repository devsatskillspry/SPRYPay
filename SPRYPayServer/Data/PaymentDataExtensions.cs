using System.Runtime.InteropServices;
using SPRYPayServer.Services.Invoices;
using Newtonsoft.Json.Linq;

namespace SPRYPayServer.Data
{
    public static class PaymentDataExtensions
    {
        public static PaymentEntity GetBlob(this PaymentData paymentData, SPRYPayNetworkProvider networks)
        {
            var unziped = ZipUtils.Unzip(paymentData.Blob);
            var cryptoCode = "SPRY";
            if (JObject.Parse(unziped).TryGetValue("cryptoCode", out var v) && v.Type == JTokenType.String)
                cryptoCode = v.Value<string>();
            var network = networks.GetNetwork<SPRYPayNetworkBase>(cryptoCode);
            PaymentEntity paymentEntity = null;
            if (network == null)
            {
                return null;
            }
            else
            {
                paymentEntity = network.ToObject<PaymentEntity>(unziped);
            }
            paymentEntity.Network = network;
            paymentEntity.Accounted = paymentData.Accounted;
            return paymentEntity;
        }
    }
}
