using System;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VaultApi.Common.Configuration;
using VaultApi.Common.HostedServices;
using VaultApi.Common.Persistence;
using VaultApi.Worker.MessageConsumers;
using Swisschain.Sdk.Server.Common;

namespace VaultApi.Worker
{
    public sealed class Startup : SwisschainStartup<AppConfig>
    {
        public Startup(IConfiguration configuration)
            : base(configuration)
        {
        }

        protected override void ConfigureServicesExt(IServiceCollection services)
        {
            base.ConfigureServicesExt(services);

            services
                .AddHttpClient()
                .AddPersistence(Config.Db.ConnectionString)
                .AddHostedService<MigrationHost>()
                .AddMessageConsumers()
                .AddMassTransit(configurator =>
                {
                    configurator.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(factoryConfigurator =>
                    {
                        factoryConfigurator.Host(Config.RabbitMq.HostUrl,
                            host =>
                            {
                                host.Username(Config.RabbitMq.Username);
                                host.Password(Config.RabbitMq.Password);
                            });

                        factoryConfigurator.UseMessageRetry(retryConfigurator =>
                            retryConfigurator.Exponential(5,
                                TimeSpan.FromMilliseconds(100),
                                TimeSpan.FromMilliseconds(10_000),
                                TimeSpan.FromMilliseconds(100)));

                        factoryConfigurator.SetLoggerFactory(provider.Container.GetRequiredService<ILoggerFactory>());

                        factoryConfigurator.ReceiveEndpoint(
                            "sirius-vault-api-blockchain-updates",
                            endpoint =>
                            {
                                endpoint.Consumer(provider.Container
                                    .GetRequiredService<BlockchainUpdatesConsumer>);
                            });

                        factoryConfigurator.ReceiveEndpoint(
                            "sirius-vault-api-transfer-validation-request-updates",
                            endpoint =>
                            {
                                endpoint.Consumer(provider.Container
                                    .GetRequiredService<TransferValidationRequestUpdatesConsumer>);
                            });

                        factoryConfigurator.ReceiveEndpoint(
                            "sirius-vault-api-transfer-signing-request-updates",
                            endpoint =>
                            {
                                endpoint.Consumer(provider.Container
                                    .GetRequiredService<TransferSigningRequestUpdatesConsumer>);
                            });

                        factoryConfigurator.ReceiveEndpoint(
                            "sirius-vault-api-vault-updates",
                            endpoint =>
                            {
                                endpoint.Consumer(provider.Container
                                    .GetRequiredService<VaultUpdatedConsumer>);
                            });

                        factoryConfigurator.ReceiveEndpoint(
                            "sirius-vault-api-wallet-generation-request-updates",
                            endpoint =>
                            {
                                endpoint.Consumer(provider.Container
                                    .GetRequiredService<WalletGenerationRequestUpdatedConsumer>);
                            });
                    }));
                })
                .AddHostedService<BusHost>();
        }
    }
}
