using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VaultApi.Common.ReadModels.KeyKeepers;
using Z.EntityFramework.Plus;

namespace VaultApi.Common.Persistence.KeyKeepers
{
    public class KeyKeepersRepository : IKeyKeepersRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public KeyKeepersRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<KeyKeeper> GetByIdOrDefaultAsync(long keyKeeperId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.KeyKeepers.FirstOrDefaultAsync(entity => entity.Id == keyKeeperId);
        }

        public async Task<KeyKeeper> GetByKeyIdOrDefaultAsync(string keyId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.KeyKeepers.FirstOrDefaultAsync(entity => entity.KeyId == keyId);
        }

        public async Task InsertOrUpdateAsync(KeyKeeper keyKeeper)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var affectedRowsCount = await context.KeyKeepers
                .Where(entity => entity.Id == keyKeeper.Id && entity.UpdatedAt <= keyKeeper.UpdatedAt)
                .UpdateAsync(entity => new KeyKeeper
                {
                    Id = keyKeeper.Id,
                    TenantId = keyKeeper.TenantId,
                    KeyId = keyKeeper.KeyId,
                    Description = keyKeeper.Description,
                    CreatedAt = keyKeeper.CreatedAt,
                    UpdatedAt = keyKeeper.UpdatedAt
                });

            if (affectedRowsCount == 0)
            {
                try
                {
                    context.KeyKeepers.Add(keyKeeper);
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
