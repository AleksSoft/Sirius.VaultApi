namespace VaultApi.Common.ReadModels.Transactions
{
    public enum TransactionSigningRequestState
    {
        Pending = 0,

        Completed = 1,

        Stale = 2,

        Rejected = 3,
        
        Approving = 4
    }
}
