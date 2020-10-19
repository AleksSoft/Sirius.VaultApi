using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VaultApi.Common.ReadModels.TransferValidationRequests;
using VaultApi.Common.ReadModels.Vaults;
using Z.EntityFramework.Plus;

namespace VaultApi.Common.Persistence.TransferValidationRequests
{
    public class TransferValidationRequestRepository : ITransferValidationRequestRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public TransferValidationRequestRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task AddOrIgnoreAsync(TransferValidationRequest transferValidationRequest)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            context.TransferValidationRequests.Add(transferValidationRequest);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException pgEx &&
                                              pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
            }
        }

        public async Task<TransferValidationRequest> GetByIdAsync(long transferValidationRequestId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var transactionSigningRequestEntity = await context
                .TransferValidationRequests
                .FirstAsync(x => x.Id == transferValidationRequestId);

            return transactionSigningRequestEntity;
        }

        public async Task<TransferValidationRequest> GetByIdOrDefaultAsync(long transferValidationRequestId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var transactionSigningRequestEntity = await context
                .TransferValidationRequests
                .FirstOrDefaultAsync(x => x.Id == transferValidationRequestId);

            return transactionSigningRequestEntity;
        }

        public async Task<IReadOnlyCollection<TransferValidationRequest>> GetAllAsync(long? cursor, int limit)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var query = context.TransferValidationRequests.AsQueryable();

            if (cursor != null)
            {
                query = query.Where(x => x.Id > cursor);
            }

            query = query.OrderBy(x => x.Id);

            query = query.Take(limit);

            await query.LoadAsync();

            return query
                .AsEnumerable()
                .ToArray();
        }

        public async Task UpdateAsync(TransferValidationRequest transferValidationRequest)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            if (transferValidationRequest.Sequence == 0)
            {
                context.TransferValidationRequests.Add(transferValidationRequest);
                await context.SaveChangesAsync();
            }
            else
            {
                var affectedRowsCount = await context.TransferValidationRequests
                    .Where(x => x.Id == transferValidationRequest.Id &&
                                x.Sequence + 1 == transferValidationRequest.Sequence)
                    .UpdateAsync(x => new TransferValidationRequest
                    {
                        Id = transferValidationRequest.Id,
                        TransferId = transferValidationRequest.TransferId,
                        TenantId = transferValidationRequest.TenantId,
                        VaultId = transferValidationRequest.VaultId,
                        VaultType = transferValidationRequest.VaultType,
                        Blockchain = transferValidationRequest.Blockchain,
                        Asset = transferValidationRequest.Asset,
                        SourceAddress = transferValidationRequest.SourceAddress,
                        DestinationAddress = transferValidationRequest.DestinationAddress,
                        Amount = transferValidationRequest.Amount,
                        FeeLimit = transferValidationRequest.FeeLimit,
                        TransferContext = transferValidationRequest.TransferContext,
                        Document = transferValidationRequest.Document,
                        Signature = transferValidationRequest.Signature,
                        RejectionReason = transferValidationRequest.RejectionReason,
                        RejectionReasonMessage = transferValidationRequest.RejectionReasonMessage,
                        State = transferValidationRequest.State,
                        Sequence = transferValidationRequest.Sequence,
                        UpdatedAt = transferValidationRequest.UpdatedAt,
                        CreatedAt = transferValidationRequest.CreatedAt
                    });

                if (affectedRowsCount != 1)
                {
                    throw new InvalidOperationException(
                        $"No rows found to update the transferValidationRequest {transferValidationRequest.Id}");
                }
            }
        }

        public async Task<IReadOnlyList<TransferValidationRequest>> GetPendingForSharedVaultAsync(string tenantId =
            null)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var query = context.TransferValidationRequests
                .Where(entity => entity.State == TransferValidationRequestState.Created)
                .Where(entity => entity.VaultType == VaultType.Shared);

            if (!string.IsNullOrEmpty(tenantId))
            {
                query = query.Where(x => x.TenantId == tenantId);
            }

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<TransferValidationRequest>> GetPendingForPrivateVaultAsync(long vaultId,
            string tenantId = null)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var query = context.TransferValidationRequests
                .Where(entity => entity.State == TransferValidationRequestState.Created)
                .Where(entity => entity.VaultType == VaultType.Private)
                .Where(entity => entity.VaultId == vaultId);

            if (!string.IsNullOrEmpty(tenantId))
            {
                query = query.Where(x => x.TenantId == tenantId);
            }

            return await query.ToListAsync();
        }
    }
}
