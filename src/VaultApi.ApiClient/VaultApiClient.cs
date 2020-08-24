using System;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Swisschain.Sirius.VaultApi.ApiContract.Monitoring;
using Swisschain.Sirius.VaultApi.ApiContract.TransactionApprovalConfirmations;
using Swisschain.Sirius.VaultApi.ApiContract.TransactionApprovalRequests;
using Swisschain.Sirius.VaultApi.ApiContract.Transactions;
using Swisschain.Sirius.VaultApi.ApiContract.Wallets;

namespace Swisschain.Sirius.VaultApi.ApiClient
{
    public class VaultApiClient : IVaultApiClient
    {
        public VaultApiClient(string apiKey, string serverGrpcUrl)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var interceptor = GrpcChannel.ForAddress(serverGrpcUrl)
                .Intercept(metadata =>
                {
                    metadata.Add("Authorization", $"Bearer {apiKey}");
                    return metadata;
                });

            Monitoring = new Monitoring.MonitoringClient(interceptor);
            TransactionApprovalConfirmations = new TransactionApprovalConfirmations.TransactionApprovalConfirmationsClient(interceptor);
            TransactionApprovalRequests = new TransactionApprovalRequests.TransactionApprovalRequestsClient(interceptor);
            Transactions = new Transactions.TransactionsClient(interceptor);
            Wallets = new Wallets.WalletsClient(interceptor);
        }

        public Monitoring.MonitoringClient Monitoring { get; }

        public TransactionApprovalConfirmations.TransactionApprovalConfirmationsClient TransactionApprovalConfirmations { get; }

        public TransactionApprovalRequests.TransactionApprovalRequestsClient TransactionApprovalRequests { get; }

        public Transactions.TransactionsClient Transactions { get; }

        public Wallets.WalletsClient Wallets { get; }
    }
}
