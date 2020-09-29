using System.Collections.Generic;
using System.Threading.Tasks;
using VaultApi.Common.ReadModels.TransferValidationRequests;

namespace VaultApi.Common.Persistence.TransferValidationRequests
{
    public interface ITransferValidationRequestRepository
    {
        Task AddOrIgnoreAsync(TransferValidationRequest transferValidationRequest);
        Task<TransferValidationRequest> GetByIdAsync(long transferValidationRequestId);
        Task<TransferValidationRequest> GetByIdOrDefaultAsync(long transferValidationRequestId);
        Task<IReadOnlyCollection<TransferValidationRequest>> GetAllAsync(long? cursor, int limit);
        Task UpdateAsync(TransferValidationRequest transferValidationRequest);
        Task<IReadOnlyList<TransferValidationRequest>> GetPendingForSharedVaultAsync();

        Task<IReadOnlyList<TransferValidationRequest>> GetPendingForPrivateVaultAsync(long vaultId);
    }
}
