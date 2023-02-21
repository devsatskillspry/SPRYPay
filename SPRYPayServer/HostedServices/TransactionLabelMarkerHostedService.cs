#nullable enable
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SPRYPayServer.Client.Models;
using SPRYPayServer.Data;
using SPRYPayServer.Events;
using SPRYPayServer.Logging;
using SPRYPayServer.Payments;
using SPRYPayServer.Payments.Bitcoin;
using SPRYPayServer.Services;
using SPRYPayServer.Services.Apps;
using SPRYPayServer.Services.Labels;
using SPRYPayServer.Services.PaymentRequests;
using NBitcoin;
using NBXplorer.DerivationStrategy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SPRYPayServer.HostedServices
{
    public class TransactionLabelMarkerHostedService : EventHostedServiceBase
    {
        private readonly WalletRepository _walletRepository;

        public SPRYPayNetworkProvider NetworkProvider { get; }

        public TransactionLabelMarkerHostedService(SPRYPayNetworkProvider networkProvider, EventAggregator eventAggregator, WalletRepository walletRepository, Logs logs) :
            base(eventAggregator, logs)
        {
            NetworkProvider = networkProvider;
            _walletRepository = walletRepository;
        }

        protected override void SubscribeToEvents()
        {
            Subscribe<InvoiceEvent>();
            Subscribe<NewOnChainTransactionEvent>();
        }
        protected override async Task ProcessEvent(object evt, CancellationToken cancellationToken)
        {
            switch (evt)
            {
                // For each new transaction that we detect, we check if we can find
                // any utxo or script object matching it.
                // If we find, then we create a link between them and the tx object.
                case NewOnChainTransactionEvent transactionEvent:
                    {
                        var network = NetworkProvider.GetNetwork<SPRYPayNetwork>(transactionEvent.CryptoCode);
                        var derivation = transactionEvent.NewTransactionEvent.DerivationStrategy;
                        if (network is null || derivation is null)
                            break;
                        var txHash = transactionEvent.NewTransactionEvent.TransactionData.TransactionHash.ToString();

                        // find all wallet objects that fit this transaction
                        // that means see if there are any utxo objects that match in/outs and scripts/addresses that match outs
                        var matchedObjects = transactionEvent.NewTransactionEvent.TransactionData.Transaction.Inputs
                            .Select<TxIn, ObjectTypeId>(txIn => new ObjectTypeId(WalletObjectData.Types.Utxo, txIn.PrevOut.ToString()))
                            .Concat(transactionEvent.NewTransactionEvent.Outputs.SelectMany<NBXplorer.Models.MatchedOutput, ObjectTypeId>(txOut =>

                                new[]{
                            new ObjectTypeId(WalletObjectData.Types.Address, GetAddress(derivation, txOut, network).ToString()),
                            new ObjectTypeId(WalletObjectData.Types.Utxo, new OutPoint(transactionEvent.NewTransactionEvent.TransactionData.TransactionHash, (uint)txOut.Index).ToString())

                                })).Distinct().ToArray();

                        var objs = await _walletRepository.GetWalletObjects(new GetWalletObjectsQuery() { TypesIds = matchedObjects });

                        foreach (var walletObjectDatas in objs.GroupBy(data => data.Key.WalletId))
                        {
                            var txWalletObject = new WalletObjectId(walletObjectDatas.Key,
                                WalletObjectData.Types.Tx, txHash);
                            await _walletRepository.EnsureWalletObject(txWalletObject);
                            foreach (var walletObjectData in walletObjectDatas)
                            {
                                await _walletRepository.EnsureWalletObjectLink(txWalletObject, walletObjectData.Key);
                            }
                        }

                        break;
                    }
                case InvoiceEvent { Name: InvoiceEvent.ReceivedPayment } invoiceEvent when
                    invoiceEvent.Payment.GetPaymentMethodId()?.PaymentType == BitcoinPaymentType.Instance &&
                    invoiceEvent.Payment.GetCryptoPaymentData() is BitcoinLikePaymentData bitcoinLikePaymentData:
                    {
                        var walletId = new WalletId(invoiceEvent.Invoice.StoreId, invoiceEvent.Payment.GetCryptoCode());
                        var transactionId = bitcoinLikePaymentData.Outpoint.Hash;
                        var labels = new List<Attachment>
                    {
                        Attachment.Invoice(invoiceEvent.Invoice.Id)
                    };
                        foreach (var paymentId in PaymentRequestRepository.GetPaymentIdsFromInternalTags(invoiceEvent.Invoice))
                        {
                            labels.Add(Attachment.PaymentRequest(paymentId));
                        }
                        foreach (var appId in AppService.GetAppInternalTags(invoiceEvent.Invoice))
                        {
                            labels.Add(Attachment.App(appId));
                        }

                        await _walletRepository.AddWalletTransactionAttachment(walletId, transactionId, labels);
                        break;
                    }
            }
        }

        private BitcoinAddress GetAddress(DerivationStrategyBase derivationStrategy, NBXplorer.Models.MatchedOutput txOut, SPRYPayNetwork network)
        {
            // Old version of NBX doesn't give address in the event, so we need to guess
            return (txOut.Address ?? network.NBXplorerNetwork.CreateAddress(derivationStrategy, txOut.KeyPath, txOut.ScriptPubKey));
        }
    }
}
