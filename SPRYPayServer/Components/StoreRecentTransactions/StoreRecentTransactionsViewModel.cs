using System.Collections.Generic;
using SPRYPayServer.Data;

namespace SPRYPayServer.Components.StoreRecentTransactions;

public class StoreRecentTransactionsViewModel
{
    public StoreData Store { get; set; }
    public IList<StoreRecentTransactionViewModel> Transactions { get; set; } = new List<StoreRecentTransactionViewModel>();
    public WalletId WalletId { get; set; }
    public bool InitialRendering { get; set; }
    public string CryptoCode { get; set; }
}
