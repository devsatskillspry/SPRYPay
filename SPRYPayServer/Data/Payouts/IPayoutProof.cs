namespace SPRYPayServer.Data
{
    public interface IPayoutProof
    {
        string ProofType { get; }
        string Link { get; }
        string Id { get; }
    }
}
