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

        public async Task Upsert(Vault vault)
        {
            int affectedRowsCount = 0;
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            if (vault.CreatedAt != vault.UpdatedAt)
            {
                affectedRowsCount = await context.Vaults
                    .Where(x => x.Id == vault.Id &&
                                x.UpdatedAt <= vault.UpdatedAt)
                    .UpdateAsync(x => new Vault
                    {
                        Id = vault.Id,
                        UpdatedAt = vault.UpdatedAt,
                        CreatedAt = vault.CreatedAt,
                        TenantId = vault.TenantId,
                        Name = vault.Name,
                        Status = vault.Status,
                        Type = vault.Type
                    });
            }

            if (affectedRowsCount == 0)
            {
                try
                {
                    context.Vaults.Add(vault);
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
