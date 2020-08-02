namespace VaultApi.Common.ReadModels.Wallets
{
    public enum WalletGenerationRequestState
    {
        Pending = 0,

        Completed = 1,

        Stale = 2,

        Rejected = 3
    }
}
