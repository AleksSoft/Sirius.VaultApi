using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VaultApi.Common.ReadModels.Blockchains;
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

        public async Task Upsert(TransactionSigningRequest transactionSigningRequest)
        {
            int affectedRowsCount = 0;
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            if (transactionSigningRequest.CreatedAt != transactionSigningRequest.UpdatedAt)
            {
                affectedRowsCount = await context.TransactionSigningRequests
                    .Where(x => x.Id == transactionSigningRequest.Id &&
                                x.UpdatedAt <= transactionSigningRequest.UpdatedAt)
                    .UpdateAsync(x => new TransactionSigningRequest
                    {
                        BlockchainId = transactionSigningRequest.BlockchainId,
                        BuiltTransaction = transactionSigningRequest.BuiltTransaction,
                        CoinsToSpend = transactionSigningRequest.CoinsToSpend,
                        Component = transactionSigningRequest.Component,
                        CreatedAt = transactionSigningRequest.CreatedAt,
                        DoubleSpendingProtectionType = transactionSigningRequest.DoubleSpendingProtectionType,
                        Id = transactionSigningRequest.Id,
                        NetworkType = transactionSigningRequest.NetworkType,
                        OperationId = transactionSigningRequest.OperationId,
                        OperationType = transactionSigningRequest.OperationType,
                        ProtocolCode = transactionSigningRequest.ProtocolCode,
                        RejectionReason = transactionSigningRequest.RejectionReason,
                        RejectionReasonMessage = transactionSigningRequest.RejectionReasonMessage,
                        SigningAddresses = transactionSigningRequest.SigningAddresses,
                        State = transactionSigningRequest.State,
                        TenantId = transactionSigningRequest.TenantId,
                        UpdatedAt = transactionSigningRequest.UpdatedAt,
                        VaultId = transactionSigningRequest.VaultId,
                        VaultType = transactionSigningRequest.VaultType
                    });
            }

            if (affectedRowsCount == 0)
            {
                try
                {
                    context.TransactionSigningRequests.Add(transactionSigningRequest);
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
