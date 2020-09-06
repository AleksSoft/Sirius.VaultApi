using System.Threading.Tasks;
using VaultApi.Common.ReadModels.KeyKeepers;

namespace VaultApi.Common.Persistence.KeyKeepers
{
    public interface IKeyKeepersRepository
    {
        Task<KeyKeeper> GetByIdOrDefaultAsync(long keyKeeperId);

        Task<KeyKeeper> GetByKeyIdOrDefaultAsync(string keyId);

        Task InsertOrUpdateAsync(KeyKeeper keyKeeper);
    }
}
