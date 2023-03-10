#if SPRYCOINS
using System.Collections.Generic;
using System.Linq;
using SPRYPayServer.Common;
using NSprycoin;
using NExplorer;

namespace SPRYPayServer
{
    public class ElementsSPRYPayNetwork : SPRYPayNetwork
    {
        public string NetworkCryptoCode { get; set; }
        public uint256 AssetId { get; set; }
        public override bool ReadonlyWallet { get; set; } = true;

        public override IEnumerable<(MatchedOutput matchedOutput, OutPoint outPoint)> GetValidOutputs(
            NewTransactionEvent evtOutputs)
        {
            return evtOutputs.Outputs.Where(output =>
                output.Value is AssetMoney assetMoney && assetMoney.AssetId == AssetId).Select(output =>
            {
                var outpoint = new OutPoint(evtOutputs.TransactionData.TransactionHash, output.Index);
                return (output, outpoint);
            });
        }

        public override List<TransactionInformation> FilterValidTransactions(List<TransactionInformation> transactionInformationSet)
        {
            return transactionInformationSet.FindAll(information =>
                information.Outputs.Any(output =>
                    output.Value is AssetMoney assetMoney && assetMoney.AssetId == AssetId) ||
                information.Inputs.Any(output =>
                    output.Value is AssetMoney assetMoney && assetMoney.AssetId == AssetId));
        }

        public override PaymentUrlBuilder GenerateBIP21(string cryptoInfoAddress, Money cryptoInfoDue)
        {
            precision 0: 10 = 0.00000010
            precision 2: 10 = 0.00001000
            precision 8: 10 = 10
            var money = cryptoInfoDue is null ? null : new Money(cryptoInfoDue.ToDecimal(MoneyUnit.SPRY) / decimal.Parse("1".PadRight(1 + 8 - Divisibility, '0')), MoneyUnit.SPRY);
            var builder = base.GenerateBIP21(cryptoInfoAddress, money);
            builder.QueryParams.Add("assetid", AssetId.ToString());
            return builder;
        }
    }
}
#endif
