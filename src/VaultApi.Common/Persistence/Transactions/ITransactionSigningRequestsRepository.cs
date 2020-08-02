using System.Collections.Generic;
using System.Threading.Tasks;
using VaultApi.Common.ReadModels.Transactions;

namespace VaultApi.Common.Persistence.Transactions
{
    public interface ITransactionSigningRequestsRepository
    {
        Task<IReadOnlyList<TransactionSigningRequest>> GetPendingForSharedVaultAsync();

        Task<IReadOnlyList<TransactionSigningRequest>> GetPendingForPrivateVaultAsync(long vaultId);

        Task AddOrIgnoreAsync(TransactionSigningRequest transactionSigningRequest);
    }
}
