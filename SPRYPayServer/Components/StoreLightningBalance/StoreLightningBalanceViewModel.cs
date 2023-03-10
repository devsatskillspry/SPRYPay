using SPRYPayServer.Data;
using SPRYPayServer.Lightning;
using SPRYPayServer.Services.Rates;
using NBitcoin;

namespace SPRYPayServer.Components.StoreLightningBalance;

public class StoreLightningBalanceViewModel
{
    public string CryptoCode { get; set; }
    public string DefaultCurrency { get; set; }
    public CurrencyData CurrencyData { get; set; }
    public StoreData Store { get; set; }
    public Money TotalOnchain { get; set; }
    public LightMoney TotalOffchain { get; set; }
    public LightningNodeBalance Balance { get; set; }
    public string ProblemDescription { get; set; }
    public bool InitialRendering { get; set; }
}
