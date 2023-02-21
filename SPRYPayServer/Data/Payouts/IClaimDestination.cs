#nullable enable

namespace SPRYPayServer.Data
{
    public interface IClaimDestination
    {
        public string? Id { get; }
        decimal? Amount { get; }
    }
}
