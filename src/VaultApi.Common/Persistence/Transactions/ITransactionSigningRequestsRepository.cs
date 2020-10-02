using System.Collections.Generic;
using System.Threading.Tasks;
using VaultApi.Common.ReadModels.Transactions;

namespace VaultApi.Common.Persistence.Transactions
{
    public interface ITransactionSigningRequestsRepository
    {
        Task<TransactionSigningRequest> GetByIdAsync(long transactionSigningRequestId);

        Task<IReadOnlyList<TransactionSigningRequest>> GetPendingForSharedVaultAsync(string tenantId = null);

        Task<IReadOnlyList<TransactionSigningRequest>> GetPendingForPrivateVaultAsync(long vaultId, string tenantId = null);

        Task InsertOrUpdateAsync(TransactionSigningRequest transactionSigningRequest);
    }
}
