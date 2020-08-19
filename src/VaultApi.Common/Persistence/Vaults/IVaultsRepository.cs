using System.Threading.Tasks;
using VaultApi.Common.ReadModels.Vaults;

namespace VaultApi.Common.Persistence.Vaults
{
    public interface IVaultsRepository
    {
        Task<Vault> GetByIdAsync(long vaultId);

        Task Upsert(Vault vault);
    }
}