namespace VaultApi.Common.ReadModels.TransferSigningRequests
{
    public enum TransferSigningRequestState
    {
        Pending = 0,

        Completed = 1,

        Stale = 2,

        Rejected = 3,
        
        Approving = 4
    }
}
