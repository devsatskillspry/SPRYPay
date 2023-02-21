using System;
using System.Globalization;
using System.Linq;
using SPRYPayServer.Abstractions.Extensions;
using SPRYPayServer.BIP78.Sender;
using SPRYPayServer.Client.Models;
using SPRYPayServer.Payments.Bitcoin;
using SPRYPayServer.Services.Invoices;
using NBitcoin;
using Newtonsoft.Json.Linq;
using InvoiceCryptoInfo = SPRYPayServer.Services.Invoices.InvoiceCryptoInfo;

namespace SPRYPayServer.Payments
{
    public class BitcoinPaymentType : PaymentType
    {
        public static BitcoinPaymentType Instance { get; } = new BitcoinPaymentType();

        private BitcoinPaymentType() { }

        public override string ToPrettyString() => "On-Chain";
        public override string GetId() => "SPRYLike";
        public override string GetBadge() => "";
        public override string ToStringNormalized() => "OnChain";
        public override CryptoPaymentData DeserializePaymentData(SPRYPayNetworkBase network, string str)
        {
            return ((SPRYPayNetwork)network)?.ToObject<BitcoinLikePaymentData>(str);
        }

        public override string SerializePaymentData(SPRYPayNetworkBase network, CryptoPaymentData paymentData)
        {
            return ((SPRYPayNetwork)network).ToString(paymentData);
        }

        public override IPaymentMethodDetails DeserializePaymentMethodDetails(SPRYPayNetworkBase network, string str)
        {
            return ((SPRYPayNetwork)network).ToObject<BitcoinLikeOnChainPaymentMethod>(str);
        }

        public override string SerializePaymentMethodDetails(SPRYPayNetworkBase network, IPaymentMethodDetails details)
        {
            return ((SPRYPayNetwork)network).ToString((BitcoinLikeOnChainPaymentMethod)details);
        }

        public override ISupportedPaymentMethod DeserializeSupportedPaymentMethod(SPRYPayNetworkBase network, JToken value)
        {
            ArgumentNullException.ThrowIfNull(network);
            ArgumentNullException.ThrowIfNull(value);
            var net = (SPRYPayNetwork)network;
            if (value is JObject jobj)
            {
                var scheme = net.NBXplorerNetwork.Serializer.ToObject<DerivationSchemeSettings>(jobj);
                scheme.Network = net;
                return scheme;
            }
            // Legacy
            return DerivationSchemeSettings.Parse(((JValue)value).Value<string>(), net);
        }

        public override string GetTransactionLink(SPRYPayNetworkBase network, string txId)
        {
            ArgumentNullException.ThrowIfNull(txId);
            if (network?.BlockExplorerLink == null)
                return null;
            txId = txId.Split('-').First();
            return string.Format(CultureInfo.InvariantCulture, network.BlockExplorerLink, txId);
        }

        public override string GetPaymentLink(SPRYPayNetworkBase network, IPaymentMethodDetails paymentMethodDetails,
            Money cryptoInfoDue, string serverUri)
        {
            if (!paymentMethodDetails.Activated)
            {
                return string.Empty;
            }
            var bip21 = ((SPRYPayNetwork)network).GenerateBIP21(paymentMethodDetails.GetPaymentDestination(), cryptoInfoDue);

            if ((paymentMethodDetails as BitcoinLikeOnChainPaymentMethod)?.PayjoinEnabled is true && serverUri != null)
            {
                bip21.QueryParams.Add(PayjoinClient.BIP21EndpointKey, $"{serverUri.WithTrailingSlash()}{network.CryptoCode}/{PayjoinClient.BIP21EndpointKey}");
            }
            return bip21.ToString();
        }

        public override string InvoiceViewPaymentPartialName { get; } = "Bitcoin/ViewBitcoinLikePaymentData";
        public override object GetGreenfieldData(ISupportedPaymentMethod supportedPaymentMethod, bool canModifyStore)
        {
            if (supportedPaymentMethod is DerivationSchemeSettings derivationSchemeSettings)
                return new OnChainPaymentMethodBaseData()
                {
                    DerivationScheme = derivationSchemeSettings.AccountDerivation.ToString(),
                    AccountKeyPath = derivationSchemeSettings.GetSigningAccountKeySettings().GetRootedKeyPath(),
                    Label = derivationSchemeSettings.Label
                };
            return null;
        }

        public override bool IsPaymentType(string paymentType)
        {
            return string.IsNullOrEmpty(paymentType) || base.IsPaymentType(paymentType);
        }

        public override void PopulateCryptoInfo(PaymentMethod details, InvoiceCryptoInfo cryptoInfo,
            string serverUrl)
        {
            cryptoInfo.PaymentUrls = new InvoiceCryptoInfo.InvoicePaymentUrls()
            {
                BIP21 = GetPaymentLink(details.Network, details.GetPaymentMethodDetails(), cryptoInfo.Due, serverUrl),
            };
        }
    }
}
