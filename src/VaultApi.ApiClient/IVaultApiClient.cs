using Swisschain.Sirius.VaultApi.ApiContract.Monitoring;
using Swisschain.Sirius.VaultApi.ApiContract.Transactions;
using Swisschain.Sirius.VaultApi.ApiContract.Wallets;

namespace Swisschain.Sirius.VaultApi.ApiClient
{
    public interface IVaultApiClient
    {
        Monitoring.MonitoringClient Monitoring { get; }

        Transactions.TransactionsClient Transactions { get; }

        Wallets.WalletsClient Wallets { get; }
    }
}
