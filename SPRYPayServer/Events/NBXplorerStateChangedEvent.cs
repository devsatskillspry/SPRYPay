using SPRYPayServer.HostedServices;

namespace SPRYPayServer.Events
{
    public class NBXplorerStateChangedEvent
    {
        public NBXplorerStateChangedEvent(SPRYPayNetworkBase network, NBXplorerState old, NBXplorerState newState)
        {
            Network = network;
            NewState = newState;
            OldState = old;
        }

        public SPRYPayNetworkBase Network { get; set; }
        public NBXplorerState NewState { get; set; }
        public NBXplorerState OldState { get; set; }

        public override string ToString()
        {
            return $"NBXplorer {Network.CryptoCode}: {OldState} => {NewState}";
        }
    }
}
