using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VaultApi.Common.Persistence.Blockchains;
using VaultApi.Common.Persistence.Transactions;
using VaultApi.Common.Persistence.Vaults;
using VaultApi.Common.Persistence.Wallets;

namespace VaultApi.Common.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
        {
            services.AddTransient<IBlockchainsRepository, BlockchainsRepository>();
            services.AddTransient<ITransactionSigningRequestsRepository, TransactionSigningRequestsRepository>();
            services.AddTransient<IVaultsRepository, VaultsRepository>();
            services.AddTransient<IWalletGenerationRequestRepository, WalletGenerationRequestRepository>();

            services.AddSingleton(x =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
                optionsBuilder
                    .UseLoggerFactory(x.GetRequiredService<ILoggerFactory>())
                    .UseNpgsql(connectionString,
                        builder => builder.MigrationsHistoryTable(
                            DatabaseContext.MigrationHistoryTable,
                            DatabaseContext.SchemaName));

                return optionsBuilder;
            });

            return services;
        }
    }
}
