using System.Collections.Generic;
using System.Threading.Tasks;
using VaultApi.Common.ReadModels.TransferSigningRequests;

namespace VaultApi.Common.Persistence.TransferSigningRequests
{
    public interface ITransferSigningRequestsRepository
    {
        Task<TransferSigningRequest> GetByIdAsync(long transferSigningRequestId);

        Task<IReadOnlyList<TransferSigningRequest>> GetPendingForSharedVaultAsync(string tenantId = null);

        Task<IReadOnlyList<TransferSigningRequest>>
            GetPendingForPrivateVaultAsync(long vaultId, string tenantId = null);

        Task InsertOrUpdateAsync(TransferSigningRequest transferSigningRequest);
    }
}
