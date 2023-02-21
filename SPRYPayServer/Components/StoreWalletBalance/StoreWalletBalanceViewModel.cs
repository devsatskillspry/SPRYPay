using System.Collections.Generic;
using SPRYPayServer.Data;
using SPRYPayServer.Services.Rates;
using SPRYPayServer.Services.Wallets;

namespace SPRYPayServer.Components.StoreWalletBalance;

public class StoreWalletBalanceViewModel
{
    public decimal? Balance { get; set; }
    public string CryptoCode { get; set; }
    public string DefaultCurrency { get; set; }
    public CurrencyData CurrencyData { get; set; }
    public StoreData Store { get; set; }
    public WalletId WalletId { get; set; }
    public WalletHistogramType Type { get; set; }
    public IList<string> Labels { get; set; }
    public IList<decimal> Series { get; set; }
}
