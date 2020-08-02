using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VaultApi.Common.ReadModels.Vaults;
using Z.EntityFramework.Plus;

namespace VaultApi.Common.Persistence.Vaults
{
    public class VaultsRepository : IVaultsRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public VaultsRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<Vault> GetByIdAsync(long vaultId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.Vaults.FindAsync(vaultId);
        }

        public async Task AddOrUpdateAsync(Vault vault)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var affectedRowsCount = await context.Vaults
                .Where(entity => entity.Id == vault.Id)
                .UpdateAsync(factory => new Vault
                {
                    Name = vault.Name,
                    Type = vault.Type,
                    Status = vault.Status,
                    UpdatedAt = vault.UpdatedAt
                });

            if (affectedRowsCount == 0)
            {
                try
                {
                    context.Vaults.Add(vault);
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
