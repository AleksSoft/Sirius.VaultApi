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

        public async Task Upsert(Blockchain blockchain)
        {
            int affectedRowsCount = 0;
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            if (blockchain.CreatedAt != blockchain.UpdatedAt)
            {
                affectedRowsCount = await context.Blockchains
                    .Where(x => x.Id == blockchain.Id &&
                                x.UpdatedAt <= blockchain.UpdatedAt)
                    .UpdateAsync(x => new Blockchain
                    {
                        NetworkType = blockchain.NetworkType,
                        UpdatedAt = blockchain.UpdatedAt,
                        Name = blockchain.Name,
                        Protocol = blockchain.Protocol,
                        TenantId = blockchain.TenantId,
                        CreatedAt = blockchain.CreatedAt,
                        Id = blockchain.Id
                    });
            }

            if (affectedRowsCount == 0)
            {
                try
                {
                    context.Blockchains.Add(blockchain);
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
