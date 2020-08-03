using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VaultApi.Common.ReadModels.Transactions;
using VaultApi.Common.ReadModels.Vaults;

namespace VaultApi.Common.Persistence.Transactions
{
    public class TransactionSigningRequestsRepository : ITransactionSigningRequestsRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public TransactionSigningRequestsRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<IReadOnlyList<TransactionSigningRequest>> GetPendingForSharedVaultAsync()
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.TransactionSigningRequests
                .Where(entity => entity.State == TransactionSigningRequestState.Pending)
                .Where(entity => entity.VaultType == VaultType.Shared)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<TransactionSigningRequest>> GetPendingForPrivateVaultAsync(long vaultId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.TransactionSigningRequests
                .Where(entity => entity.State == TransactionSigningRequestState.Pending)
                .Where(entity => entity.VaultType == VaultType.Private)
                .Where(entity => entity.VaultId == vaultId)
                .ToListAsync();
        }

        public async Task AddOrIgnoreAsync(TransactionSigningRequest transactionSigningRequest)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            try
            {
                context.TransactionSigningRequests.Add(transactionSigningRequest);
                await context.SaveChangesAsync();
            }
            catch (Exception exception) when (exception.InnerException is PostgresException pgException &&
                                              pgException.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                context.TransactionSigningRequests.Update(transactionSigningRequest);
                await context.SaveChangesAsync();
            }
        }
    }
}
