using System;
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

        public async Task Upsert(WalletGenerationRequest walletGenerationRequest)
        {
            int affectedRowsCount = 0;
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            if (walletGenerationRequest.CreatedAt != walletGenerationRequest.UpdatedAt)
            {
                affectedRowsCount = await context.WalletGenerationRequests
                    .Where(x => x.Id == walletGenerationRequest.Id &&
                                x.UpdatedAt <= walletGenerationRequest.UpdatedAt)
                    .UpdateAsync(x => new WalletGenerationRequest
                    {
                        Id = walletGenerationRequest.Id,
                        CreatedAt = walletGenerationRequest.CreatedAt,
                        UpdatedAt = walletGenerationRequest.UpdatedAt,
                        VaultType = walletGenerationRequest.VaultType,
                        BlockchainId = walletGenerationRequest.BlockchainId,
                        VaultId = walletGenerationRequest.VaultId,
                        NetworkType = walletGenerationRequest.NetworkType,
                        ProtocolCode = walletGenerationRequest.ProtocolCode,
                        Component = walletGenerationRequest.Component,
                        State = walletGenerationRequest.State,
                        RejectionReason = walletGenerationRequest.RejectionReason,
                        RejectionReasonMessage = walletGenerationRequest.RejectionReasonMessage,
                        TenantId = walletGenerationRequest.TenantId
                    });
            }

            if (affectedRowsCount == 0)
            {
                try
                {
                    context.WalletGenerationRequests.Add(walletGenerationRequest);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException e) when (e.InnerException is PostgresException pgEx
                                                  && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    //Swallow error: the entity was already added
                }
            }
        }
    }
}
