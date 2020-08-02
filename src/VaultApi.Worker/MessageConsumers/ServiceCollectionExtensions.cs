using Microsoft.Extensions.DependencyInjection;

namespace VaultApi.Worker.MessageConsumers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageConsumers(this IServiceCollection services)
        {
            services.AddTransient<BlockchainUpdatesConsumer>();
            services.AddTransient<TransactionSigningRequestUpdatesConsumer>();
            services.AddTransient<VaultUpdatedConsumer>();
            services.AddTransient<WalletGenerationRequestUpdatedConsumer>();

            return services;
        }
    }
}
