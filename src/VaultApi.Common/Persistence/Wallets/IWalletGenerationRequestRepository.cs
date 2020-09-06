using System.Collections.Generic;
using System.Threading.Tasks;
using VaultApi.Common.ReadModels.Wallets;

namespace VaultApi.Common.Persistence.Wallets
{
    public interface IWalletGenerationRequestRepository
    {
        Task<WalletGenerationRequest> GetByIdAsync(long walletGenerationRequestId);

        Task<IReadOnlyList<WalletGenerationRequest>> GetPendingForSharedVaultAsync();

        Task<IReadOnlyList<WalletGenerationRequest>> GetPendingForPrivateVaultAsync(long vaultId);

        Task InsertOrUpdateAsync(WalletGenerationRequest walletGenerationRequest);
    }
}
