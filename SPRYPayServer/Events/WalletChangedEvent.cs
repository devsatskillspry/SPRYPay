using System;

namespace SPRYPayServer.Events
{
    public class WalletChangedEvent
    {
        public WalletId WalletId { get; set; }
        public override string ToString()
        {
            return $"Wallet {WalletId} changed";
        }
    }
}
