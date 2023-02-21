using System.Threading.Tasks;
using NBitcoin;

namespace SPRYPayServer.Services.Fees
{
    public class FixedFeeProvider : IFeeProvider, IFeeProviderFactory
    {
        public FixedFeeProvider(FeeRate feeRate)
        {
            FeeRate = feeRate;
        }

        public FeeRate FeeRate
        {
            get; set;
        }

        public IFeeProvider CreateFeeProvider(SPRYPayNetworkBase network)
        {
            return new FixedFeeProvider(FeeRate);
        }

        public Task<FeeRate> GetFeeRateAsync(int blockTarget)
        {
            return Task.FromResult(FeeRate);
        }
    }
}
