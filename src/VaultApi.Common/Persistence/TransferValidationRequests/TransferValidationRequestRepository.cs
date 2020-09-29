using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VaultApi.Common.ReadModels.TransferValidationRequests;

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
            catch (DbUpdateException e) when (e.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
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
            }
            else
            {
                context.TransferValidationRequests.Update(transferValidationRequest);
                context.Entry(transferValidationRequest).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();
        }
    }
}
