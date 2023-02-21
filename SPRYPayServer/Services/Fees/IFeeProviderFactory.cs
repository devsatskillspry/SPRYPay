namespace SPRYPayServer.Services
{
    public interface IFeeProviderFactory
    {
        IFeeProvider CreateFeeProvider(SPRYPayNetworkBase network);
    }
}
