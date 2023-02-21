using SPRYPayServer.PayoutProcessors.Settings;

namespace SPRYPayServer.PayoutProcessors.OnChain;

public class OnChainAutomatedPayoutBlob : AutomatedPayoutBlob
{
    public int FeeTargetBlock { get; set; } = 1;
}
