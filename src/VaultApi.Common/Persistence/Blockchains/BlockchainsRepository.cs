using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VaultApi.Common.ReadModels.Blockchains;
using Z.EntityFramework.Plus;

namespace VaultApi.Common.Persistence.Blockchains
{
    public class BlockchainsRepository : IBlockchainsRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public BlockchainsRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<Blockchain> GetByIdAsync(string blockchainId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.Blockchains.FindAsync(blockchainId);
        }

        public async Task AddOrUpdateAsync(Blockchain blockchain)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var affectedRowsCount = await context.Blockchains
                .Where(entity => entity.Id == blockchain.Id)
                .UpdateAsync(factory => new Blockchain
                {
                    Name = blockchain.Name,
                    Protocol = blockchain.Protocol,
                    NetworkType = blockchain.NetworkType,
                    UpdatedAt = blockchain.UpdatedAt
                });

            if (affectedRowsCount == 0)
            {
                try
                {
                    context.Blockchains.Add(blockchain);
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
}
