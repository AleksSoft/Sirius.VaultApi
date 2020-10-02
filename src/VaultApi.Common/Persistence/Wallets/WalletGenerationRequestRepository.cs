using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VaultApi.Common.ReadModels.Vaults;
using VaultApi.Common.ReadModels.Wallets;
using Z.EntityFramework.Plus;

namespace VaultApi.Common.Persistence.Wallets
{
    public class WalletGenerationRequestRepository : IWalletGenerationRequestRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public WalletGenerationRequestRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<WalletGenerationRequest> GetByIdAsync(long walletGenerationRequestId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.WalletGenerationRequests.FindAsync(walletGenerationRequestId);
        }

        public async Task<IReadOnlyList<WalletGenerationRequest>> GetPendingForSharedVaultAsync()
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.WalletGenerationRequests
                .Where(entity => entity.State == WalletGenerationRequestState.Pending)
                .Where(entity => entity.VaultType == VaultType.Shared)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<WalletGenerationRequest>> GetPendingForPrivateVaultAsync(long vaultId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.WalletGenerationRequests
                .Where(entity => entity.State == WalletGenerationRequestState.Pending)
                .Where(entity => entity.VaultType == VaultType.Private)
                .Where(entity => entity.VaultId == vaultId)
                .ToListAsync();
        }

        public async Task InsertOrUpdateAsync(WalletGenerationRequest walletGenerationRequest)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var affectedRowsCount = await context.WalletGenerationRequests
                .Where(entity => entity.Id == walletGenerationRequest.Id &&
                                 entity.UpdatedAt <= walletGenerationRequest.UpdatedAt)
                .UpdateAsync(x => new WalletGenerationRequest
                {
                    Id = walletGenerationRequest.Id,
                    TenantId = walletGenerationRequest.TenantId,
                    VaultId = walletGenerationRequest.VaultId,
                    VaultType = walletGenerationRequest.VaultType,
                    BlockchainId = walletGenerationRequest.BlockchainId,
                    NetworkType = walletGenerationRequest.NetworkType,
                    ProtocolCode = walletGenerationRequest.ProtocolCode,
                    Component = walletGenerationRequest.Component,
                    State = walletGenerationRequest.State,
                    RejectionReason = walletGenerationRequest.RejectionReason,
                    RejectionReasonMessage = walletGenerationRequest.RejectionReasonMessage,
                    CreatedAt = walletGenerationRequest.CreatedAt,
                    UpdatedAt = walletGenerationRequest.UpdatedAt
                });

            if (affectedRowsCount == 0)
            {
                try
                {
                    context.WalletGenerationRequests.Add(walletGenerationRequest);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException exception) when (exception.InnerException is PostgresException pgException &&
                                                          pgException.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    // ignore
                }
            }
        }
    }
}
