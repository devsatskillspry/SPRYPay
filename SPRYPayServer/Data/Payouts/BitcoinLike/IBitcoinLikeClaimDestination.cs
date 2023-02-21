using NBitcoin;

namespace SPRYPayServer.Data
{
    public interface IBitcoinLikeClaimDestination : IClaimDestination
    {
        BitcoinAddress Address { get; }
    }
}
