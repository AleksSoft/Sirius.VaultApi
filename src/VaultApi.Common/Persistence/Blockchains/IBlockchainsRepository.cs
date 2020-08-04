using System.Threading.Tasks;
using VaultApi.Common.ReadModels.Blockchains;

namespace VaultApi.Common.Persistence.Blockchains
{
    public interface IBlockchainsRepository
    {
        Task<Blockchain> GetByIdAsync(string blockchainId);

        Task AddOrUpdateAsync(Blockchain blockchain);
    }
}
