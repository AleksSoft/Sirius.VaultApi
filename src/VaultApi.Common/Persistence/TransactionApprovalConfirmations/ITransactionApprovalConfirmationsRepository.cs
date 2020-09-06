using System.Collections.Generic;
using System.Threading.Tasks;
using VaultApi.Common.ReadModels.TransactionApprovalConfirmations;

namespace VaultApi.Common.Persistence.TransactionApprovalConfirmations
{
    public interface ITransactionApprovalConfirmationsRepository
    {
        Task<IReadOnlyList<TransactionApprovalConfirmation>> GetByTransactionApprovalRequestIdAsync(
            long transactionApprovalRequestId);

        Task InsertOrIgnoreAsync(TransactionApprovalConfirmation transactionApprovalConfirmation);
    }
}
