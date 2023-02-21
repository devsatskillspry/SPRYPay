using System.Threading.Tasks;
using NBitcoin;

namespace SPRYPayServer.Services
{
    public interface IFeeProvider
    {
        Task<FeeRate> GetFeeRateAsync(int blockTarget = 20);
    }
}
