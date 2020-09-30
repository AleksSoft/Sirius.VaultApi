using Microsoft.Extensions.DependencyInjection;

namespace VaultApi.Worker.MessageConsumers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageConsumers(this IServiceCollection services)
        {
            services.AddTransient<BlockchainUpdatesConsumer>();
            services.AddTransient<KeyKeeperUpdatedConsumer>();
            services.AddTransient<TransactionSigningRequestUpdatesConsumer>();
            services.AddTransient<VaultUpdatedConsumer>();
            services.AddTransient<WalletGenerationRequestUpdatedConsumer>();
            services.AddTransient<TransferValidationRequestUpdatesConsumer>();

            return services;
        }
    }
}
