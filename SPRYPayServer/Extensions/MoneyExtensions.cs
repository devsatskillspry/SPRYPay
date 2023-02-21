using System;
using System.Globalization;
using System.Linq;
using NBitcoin;
using NBXplorer;

namespace SPRYPayServer
{
    public static class MoneyExtensions
    {
        public static decimal GetValue(this IMoney m, SPRYPayNetwork network = null)
        {
            switch (m)
            {
                case Money money:
                    return money.ToDecimal(MoneyUnit.SPRY);
                case MoneyBag mb:
                    return mb.Select(money => money.GetValue(network)).Sum();
#if ALTCOINS
                case AssetMoney assetMoney:
                    if (network is ElementsSPRYPayNetwork elementsSPRYPayNetwork)
                    {
                        return elementsSPRYPayNetwork.AssetId == assetMoney.AssetId
                            ? Convert(assetMoney.Quantity, elementsSPRYPayNetwork.Divisibility)
                            : 0;
                    }
                    throw new NotSupportedException("IMoney type not supported");
#endif
                default:
                    throw new NotSupportedException("IMoney type not supported");
            }
        }

        public static decimal Convert(long sats, int divisibility = 8)
        {
            var negative = sats < 0;
            var amt = sats.ToString(CultureInfo.InvariantCulture)
                .Replace("-", "", StringComparison.InvariantCulture)
                .PadLeft(divisibility, '0');
            amt = amt.Length == divisibility ? $"0.{amt}" : amt.Insert(amt.Length - divisibility, ".");
            return decimal.Parse($"{(negative ? "-" : string.Empty)}{amt}", CultureInfo.InvariantCulture);
        }
        public static string ShowMoney(this IMoney money, SPRYPayNetwork network)
        {
            return money.GetValue(network).ShowMoney(network.Divisibility);
        }

        public static string ShowMoney(this Money money, int? divisibility)
        {
            return !divisibility.HasValue
                ? money.ToString()
                : money.ToDecimal(MoneyUnit.SPRY).ShowMoney(divisibility.Value);
        }

        public static string ShowMoney(this decimal d, int divisibility)
        {
            return d.ToString(GetDecimalFormat(divisibility), CultureInfo.InvariantCulture);
        }

        private static string GetDecimalFormat(int divisibility)
        {
            var res = $"0{(divisibility > 0 ? "." : string.Empty)}";
            return res.PadRight(divisibility + res.Length, '0');
        }
    }
}
