﻿using System;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Swisschain.Sirius.VaultApi.ApiContract.Monitoring;
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
            Transactions = new Transactions.TransactionsClient(interceptor);
            Wallets = new Wallets.WalletsClient(interceptor);
        }

        public Monitoring.MonitoringClient Monitoring { get; }

        public Transactions.TransactionsClient Transactions { get; }

        public Wallets.WalletsClient Wallets { get; }
    }
}