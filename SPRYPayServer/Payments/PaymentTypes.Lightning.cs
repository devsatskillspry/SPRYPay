using System;
using SPRYPayServer.Client.Models;
using SPRYPayServer.Controllers.Greenfield;
using SPRYPayServer.Payments.Lightning;
using SPRYPayServer.Services.Invoices;
using NBitcoin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SPRYPayServer.Payments
{
    public class LightningPaymentType : PaymentType
    {
        public static LightningPaymentType Instance { get; } = new LightningPaymentType();

        private protected LightningPaymentType() { }

        public override string ToPrettyString() => "Off-Chain";
        public override string GetId() => "LightningLike";
        public override string GetBadge() => "⚡";
        public override string ToStringNormalized() => "LightningNetwork";

        public override CryptoPaymentData DeserializePaymentData(SPRYPayNetworkBase network, string str)
        {
            return ((SPRYPayNetwork)network)?.ToObject<LightningLikePaymentData>(str);
        }

        public override string SerializePaymentData(SPRYPayNetworkBase network, CryptoPaymentData paymentData)
        {
            return ((SPRYPayNetwork)network).ToString(paymentData);
        }

        public override IPaymentMethodDetails DeserializePaymentMethodDetails(SPRYPayNetworkBase network, string str)
        {
            return JsonConvert.DeserializeObject<LightningLikePaymentMethodDetails>(str);
        }

        public override string SerializePaymentMethodDetails(SPRYPayNetworkBase network, IPaymentMethodDetails details)
        {
            return JsonConvert.SerializeObject(details);
        }

        public override ISupportedPaymentMethod DeserializeSupportedPaymentMethod(SPRYPayNetworkBase network,
            JToken value)
        {
            return JsonConvert.DeserializeObject<LightningSupportedPaymentMethod>(value.ToString());
        }

        public override string GetTransactionLink(SPRYPayNetworkBase network, string txId)
        {
            return null;
        }

        public override string GetPaymentLink(SPRYPayNetworkBase network, IPaymentMethodDetails paymentMethodDetails,
            Money cryptoInfoDue, string serverUri)
        {
            if (!paymentMethodDetails.Activated)
            {
                return string.Empty;
            }
            var lnInvoiceTrimmedOfScheme = paymentMethodDetails.GetPaymentDestination().ToLowerInvariant()
                .Replace("lightning:", "", StringComparison.InvariantCultureIgnoreCase);

            return $"lightning:{lnInvoiceTrimmedOfScheme}";
        }

        public override string InvoiceViewPaymentPartialName { get; } = "Lightning/ViewLightningLikePaymentData";

        public override object GetGreenfieldData(ISupportedPaymentMethod supportedPaymentMethod, bool canModifyStore)
        {
            if (supportedPaymentMethod is LightningSupportedPaymentMethod lightningSupportedPaymentMethod)
                return new LightningNetworkPaymentMethodBaseData()
                {
                    ConnectionString = lightningSupportedPaymentMethod.IsInternalNode
                        ?
                        lightningSupportedPaymentMethod.GetDisplayableConnectionString()
                        :
                        canModifyStore
                            ? lightningSupportedPaymentMethod.GetDisplayableConnectionString()
                            :
                            "*NEED CanModifyStoreSettings PERMISSION TO VIEW*"
                };
            return null;
        }

        public override bool IsPaymentType(string paymentType)
        {
            return paymentType?.Equals("offchain", StringComparison.InvariantCultureIgnoreCase) is true || base.IsPaymentType(paymentType);
        }

        public override void PopulateCryptoInfo(PaymentMethod details, InvoiceCryptoInfo invoiceCryptoInfo, string serverUrl)
        {
            invoiceCryptoInfo.PaymentUrls = new InvoiceCryptoInfo.InvoicePaymentUrls()
            {
                BOLT11 = GetPaymentLink(details.Network, details.GetPaymentMethodDetails(), invoiceCryptoInfo.Due,
                    serverUrl)
            };
        }
    }
}
