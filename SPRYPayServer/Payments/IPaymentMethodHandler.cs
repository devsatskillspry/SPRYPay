using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SPRYPayServer.Data;
using SPRYPayServer.Logging;
using SPRYPayServer.Models.InvoicingModels;
using SPRYPayServer.Rating;
using SPRYPayServer.Services.Invoices;
using SPRYPayServer.Services.Rates;
using NBitcoin;
using InvoiceResponse = SPRYPayServer.Models.InvoiceResponse;

namespace SPRYPayServer.Payments
{
    /// <summary>
    /// This class customize invoice creation by the creation of payment details for the PaymentMethod during invoice creation
    /// </summary>
    public interface IPaymentMethodHandler
    {
        /// <summary>
        /// Create needed to track payments of this invoice
        /// </summary>
        /// <param name="logs"></param>
        /// <param name="supportedPaymentMethod"></param>
        /// <param name="paymentMethod"></param>
        /// <param name="store"></param>
        /// <param name="network"></param>
        /// <param name="preparePaymentObject"></param>
        /// <param name="invoicePaymentMethods"></param>
        /// <returns></returns>
        Task<IPaymentMethodDetails> CreatePaymentMethodDetails(InvoiceLogs logs,
            ISupportedPaymentMethod supportedPaymentMethod,
            PaymentMethod paymentMethod, StoreData store, SPRYPayNetworkBase network, object preparePaymentObject,
            IEnumerable<PaymentMethodId> invoicePaymentMethods);

        /// <summary>
        /// This method called before the rate have been fetched
        /// </summary>
        /// <param name="supportedPaymentMethod"></param>
        /// <param name="store"></param>
        /// <param name="network"></param>
        /// <returns></returns>
        object PreparePayment(ISupportedPaymentMethod supportedPaymentMethod, StoreData store, SPRYPayNetworkBase network);

        void PreparePaymentModel(PaymentModel model, InvoiceResponse invoiceResponse, StoreBlob storeBlob,
            IPaymentMethod paymentMethod);
        string GetCryptoImage(PaymentMethodId paymentMethodId);
        string GetPaymentMethodName(PaymentMethodId paymentMethodId);

        IEnumerable<PaymentMethodId> GetSupportedPaymentMethods();
        CheckoutUIPaymentMethodSettings GetCheckoutUISettings();
    }

    public interface IPaymentMethodHandler<TSupportedPaymentMethod, TSPRYPayNetwork> : IPaymentMethodHandler
        where TSupportedPaymentMethod : ISupportedPaymentMethod
        where TSPRYPayNetwork : SPRYPayNetworkBase
    {
        Task<IPaymentMethodDetails> CreatePaymentMethodDetails(InvoiceLogs logs, TSupportedPaymentMethod supportedPaymentMethod,
            PaymentMethod paymentMethod, StoreData store, TSPRYPayNetwork network, object preparePaymentObject, IEnumerable<PaymentMethodId> invoicePaymentMethods);
    }

    public abstract class PaymentMethodHandlerBase<TSupportedPaymentMethod, TSPRYPayNetwork> : IPaymentMethodHandler<
            TSupportedPaymentMethod, TSPRYPayNetwork>
        where TSupportedPaymentMethod : ISupportedPaymentMethod
        where TSPRYPayNetwork : SPRYPayNetworkBase
    {
        public abstract PaymentType PaymentType { get; }

        public abstract Task<IPaymentMethodDetails> CreatePaymentMethodDetails(
            InvoiceLogs logs,
            TSupportedPaymentMethod supportedPaymentMethod,
            PaymentMethod paymentMethod, StoreData store, TSPRYPayNetwork network, object preparePaymentObject, IEnumerable<PaymentMethodId> invoicePaymentMethods);

        public abstract void PreparePaymentModel(PaymentModel model, InvoiceResponse invoiceResponse,
            StoreBlob storeBlob, IPaymentMethod paymentMethod);
        public abstract string GetCryptoImage(PaymentMethodId paymentMethodId);
        public abstract string GetPaymentMethodName(PaymentMethodId paymentMethodId);

        public abstract IEnumerable<PaymentMethodId> GetSupportedPaymentMethods();
        public virtual CheckoutUIPaymentMethodSettings GetCheckoutUISettings()
        {
            return new CheckoutUIPaymentMethodSettings()
            {
                ExtensionPartial = "Bitcoin/BitcoinLikeMethodCheckout",
                CheckoutBodyVueComponentName = "BitcoinLikeMethodCheckout",
                CheckoutHeaderVueComponentName = "BitcoinLikeMethodCheckoutHeader",
                NoScriptPartialName = "Bitcoin/BitcoinLikeMethodCheckoutNoScript"
            };
        }

        public PaymentMethod GetPaymentMethodInInvoice(InvoiceEntity invoice, PaymentMethodId paymentMethodId)
        {
            return invoice.GetPaymentMethod(paymentMethodId);
        }

        public virtual object PreparePayment(TSupportedPaymentMethod supportedPaymentMethod, StoreData store,
            SPRYPayNetworkBase network)
        {
            return null;
        }

        public Task<IPaymentMethodDetails> CreatePaymentMethodDetails(InvoiceLogs logs,
            ISupportedPaymentMethod supportedPaymentMethod, PaymentMethod paymentMethod,
            StoreData store, SPRYPayNetworkBase network, object preparePaymentObject,
            IEnumerable<PaymentMethodId> invoicePaymentMethods)
        {
            if (supportedPaymentMethod is TSupportedPaymentMethod method && network is TSPRYPayNetwork correctNetwork)
            {
                return CreatePaymentMethodDetails(logs, method, paymentMethod, store, correctNetwork, preparePaymentObject, invoicePaymentMethods);
            }

            throw new NotSupportedException("Invalid supportedPaymentMethod");
        }

        object IPaymentMethodHandler.PreparePayment(ISupportedPaymentMethod supportedPaymentMethod, StoreData store,
            SPRYPayNetworkBase network)
        {
            if (supportedPaymentMethod is TSupportedPaymentMethod method)
            {
                return PreparePayment(method, store, network);
            }

            throw new NotSupportedException("Invalid supportedPaymentMethod");
        }
    }
}
