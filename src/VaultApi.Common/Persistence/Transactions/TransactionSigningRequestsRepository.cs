using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VaultApi.Common.ReadModels.Transactions;
using VaultApi.Common.ReadModels.Vaults;
using Z.EntityFramework.Plus;

namespace VaultApi.Common.Persistence.Transactions
{
    public class TransactionSigningRequestsRepository : ITransactionSigningRequestsRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public TransactionSigningRequestsRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<TransactionSigningRequest> GetByIdAsync(long transactionSigningRequestId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.TransactionSigningRequests.FindAsync(transactionSigningRequestId);
        }

        public async Task<IReadOnlyList<TransactionSigningRequest>> GetPendingForSharedVaultAsync(string tenantId = null)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var query = context.TransactionSigningRequests
                .Where(entity => entity.State == TransactionSigningRequestState.Pending)
                .Where(entity => entity.VaultType == VaultType.Shared);

            if (tenantId != null)
            {
                query = query.Where(x => tenantId == x.TenantId);
            }

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<TransactionSigningRequest>> GetPendingForPrivateVaultAsync(long vaultId, string tenantId = null)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var query =context.TransactionSigningRequests
                .Where(entity => entity.State == TransactionSigningRequestState.Pending)
                .Where(entity => entity.VaultType == VaultType.Private)
                .Where(entity => entity.VaultId == vaultId);

            if (tenantId != null)
            {
                query = query.Where(x => tenantId == x.TenantId);
            }

            return await query.ToListAsync();
        }

        public async Task InsertOrUpdateAsync(TransactionSigningRequest transactionSigningRequest)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var affectedRowsCount = await context.TransactionSigningRequests
                .Where(entity => entity .Id == transactionSigningRequest.Id &&
                                 entity .UpdatedAt <= transactionSigningRequest.UpdatedAt)
                .UpdateAsync(x => new TransactionSigningRequest
                {
                    Id = transactionSigningRequest.Id,
                    TenantId = transactionSigningRequest.TenantId,
                    VaultId = transactionSigningRequest.VaultId,
                    VaultType = transactionSigningRequest.VaultType,
                    Component = transactionSigningRequest.Component,
                    OperationId = transactionSigningRequest.OperationId,
                    OperationType = transactionSigningRequest.OperationType,
                    BlockchainId = transactionSigningRequest.BlockchainId,
                    State = transactionSigningRequest.State,
                    RejectionReasonMessage = transactionSigningRequest.RejectionReasonMessage,
                    RejectionReason = transactionSigningRequest.RejectionReason,
                    NetworkType = transactionSigningRequest.NetworkType,
                    ProtocolCode = transactionSigningRequest.ProtocolCode,
                    DoubleSpendingProtectionType = transactionSigningRequest.DoubleSpendingProtectionType,
                    BuiltTransaction = transactionSigningRequest.BuiltTransaction,
                    SigningAddresses = transactionSigningRequest.SigningAddresses,
                    CoinsToSpend = transactionSigningRequest.CoinsToSpend,
                    CreatedAt = transactionSigningRequest.CreatedAt,
                    UpdatedAt = transactionSigningRequest.UpdatedAt,
                });

            if (affectedRowsCount == 0)
            {
                try
                {
                    context.TransactionSigningRequests.Add(transactionSigningRequest);
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
