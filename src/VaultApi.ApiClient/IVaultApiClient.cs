using Swisschain.Sirius.VaultApi.ApiContract.Monitoring;
using Swisschain.Sirius.VaultApi.ApiContract.TransferSigninRequests;
using Swisschain.Sirius.VaultApi.ApiContract.TransferValidationRequests;
using Swisschain.Sirius.VaultApi.ApiContract.Wallets;

namespace Swisschain.Sirius.VaultApi.ApiClient
{
    public interface IVaultApiClient
    {
        Monitoring.MonitoringClient Monitoring { get; }

        TransferSigningRequests.TransferSigningRequestsClient TransferSigningRequests { get; }

        TransferValidationRequests.TransferValidationRequestsClient TransferValidationRequests { get; }

        Wallets.WalletsClient Wallets { get; }
    }
}
