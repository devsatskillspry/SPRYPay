using SPRYPayServer.Abstractions.Custodians.Client;

namespace SPRYPayServer.Models.CustodianAccountViewModels;

public class TradePrepareViewModel : AssetQuoteResult
{
    public decimal MaxQtyToTrade { get; set; }

}
