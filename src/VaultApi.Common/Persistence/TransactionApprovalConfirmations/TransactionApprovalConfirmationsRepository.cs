using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VaultApi.Common.ReadModels.TransactionApprovalConfirmations;

namespace VaultApi.Common.Persistence.TransactionApprovalConfirmations
{
    public class TransactionApprovalConfirmationsRepository : ITransactionApprovalConfirmationsRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public TransactionApprovalConfirmationsRepository(
            DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<IReadOnlyList<TransactionApprovalConfirmation>> GetByTransactionApprovalRequestIdAsync(
            long transactionApprovalRequestId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.TransactionApprovalConfirmations
                .Where(entity => entity.TransactionApprovalRequestId == transactionApprovalRequestId)
                .ToListAsync();
        }

        public async Task InsertOrIgnoreAsync(TransactionApprovalConfirmation transactionApprovalConfirmation)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            try
            {
                context.TransactionApprovalConfirmations.Add(transactionApprovalConfirmation);
                await context.SaveChangesAsync();
            }
            catch (Exception exception) when (exception.InnerException is PostgresException pgException &&
                                              pgException.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                // ignore
            }
        }
    }
}
