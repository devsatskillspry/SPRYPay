using System.Collections.Generic;
using SPRYPayServer;
using NBitcoin;

namespace SPRYPayServer.Models.WalletViewModels
{
    public class ListWalletsViewModel
    {
        public class WalletViewModel
        {
            public string StoreName { get; set; }
            public string StoreId { get; set; }
            public string CryptoCode { get; set; }
            public string Balance { get; set; }
            public bool IsOwner { get; set; }
            public WalletId Id { get; set; }
        }

        public Dictionary<SPRYPayNetwork, IMoney> BalanceForCryptoCode { get; set; } = new Dictionary<SPRYPayNetwork, IMoney>();
        public List<WalletViewModel> Wallets { get; set; } = new List<WalletViewModel>();
    }
}
