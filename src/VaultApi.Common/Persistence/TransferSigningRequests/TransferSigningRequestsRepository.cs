using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VaultApi.Common.ReadModels.TransferSigningRequests;
using VaultApi.Common.ReadModels.Vaults;
using Z.EntityFramework.Plus;

namespace VaultApi.Common.Persistence.TransferSigningRequests
{
    public class TransferSigningRequestsRepository : ITransferSigningRequestsRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public TransferSigningRequestsRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<TransferSigningRequest> GetByIdAsync(long transferSigningRequestId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.TransferSigningRequests.FindAsync(transferSigningRequestId);
        }

        public async Task<IReadOnlyList<TransferSigningRequest>> GetPendingForSharedVaultAsync(string tenantId = null)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var query = context.TransferSigningRequests
                .Where(entity => entity.State == TransferSigningRequestState.Pending)
                .Where(entity => entity.VaultType == VaultType.Shared);

            if (tenantId != null)
            {
                query = query.Where(x => tenantId == x.TenantId);
            }

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<TransferSigningRequest>> GetPendingForPrivateVaultAsync(long vaultId,
            string tenantId = null)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var query = context.TransferSigningRequests
                .Where(entity => entity.State == TransferSigningRequestState.Pending)
                .Where(entity => entity.VaultType == VaultType.Private)
                .Where(entity => entity.VaultId == vaultId);

            if (!string.IsNullOrEmpty(tenantId))
            {
                query = query.Where(x => tenantId == x.TenantId);
            }

            return await query.ToListAsync();
        }

        public async Task InsertOrUpdateAsync(TransferSigningRequest transferSigningRequest)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var affectedRowsCount = await context.TransferSigningRequests
                .Where(entity => entity.Id == transferSigningRequest.Id &&
                                 entity.UpdatedAt <= transferSigningRequest.UpdatedAt)
                .UpdateAsync(x => new TransferSigningRequest
                {
                    Id = transferSigningRequest.Id,
                    TransferId = transferSigningRequest.TransferId,
                    TenantId = transferSigningRequest.TenantId,
                    VaultId = transferSigningRequest.VaultId,
                    VaultType = transferSigningRequest.VaultType,
                    Blockchain = transferSigningRequest.Blockchain,
                    BuiltTransaction = transferSigningRequest.BuiltTransaction,
                    SigningAddresses = transferSigningRequest.SigningAddresses,
                    CoinsToSpend = transferSigningRequest.CoinsToSpend,
                    RejectionReason = transferSigningRequest.RejectionReason,
                    RejectionReasonMessage = transferSigningRequest.RejectionReasonMessage,
                    State = transferSigningRequest.State,
                    Document = transferSigningRequest.Document,
                    Signature = transferSigningRequest.Signature,
                    Group = transferSigningRequest.Group,
                    Sequence = transferSigningRequest.Sequence,
                    CreatedAt = transferSigningRequest.CreatedAt,
                    UpdatedAt = transferSigningRequest.UpdatedAt
                });

            if (affectedRowsCount == 0)
            {
                try
                {
                    context.TransferSigningRequests.Add(transferSigningRequest);
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
