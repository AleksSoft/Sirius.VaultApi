using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VaultApi.Common.ReadModels.Vaults;
using VaultApi.Common.ReadModels.Wallets;

namespace VaultApi.Common.Persistence.Wallets
{
    public class WalletGenerationRequestRepository : IWalletGenerationRequestRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public WalletGenerationRequestRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
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

        public async Task AddOrUpdateAsync(WalletGenerationRequest walletGenerationRequest)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            try
            {
                context.WalletGenerationRequests.Add(walletGenerationRequest);
                await context.SaveChangesAsync();
            }
            catch (Exception exception) when (exception.InnerException is PostgresException pgException &&
                                              pgException.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                context.WalletGenerationRequests.Update(walletGenerationRequest);
                await context.SaveChangesAsync();
            }
        }
    }
}
