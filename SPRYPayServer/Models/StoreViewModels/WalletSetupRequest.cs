using NBXplorer.Models;

namespace SPRYPayServer.Models.StoreViewModels
{
    public class WalletSetupRequest : GenerateWalletRequest
    {
        public bool PayJoinEnabled { get; set; }
        public bool CanUsePayJoin { get; set; }
    }
}
