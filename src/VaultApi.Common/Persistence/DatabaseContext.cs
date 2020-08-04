using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using VaultApi.Common.ReadModels.Blockchains;
using VaultApi.Common.ReadModels.Transactions;
using VaultApi.Common.ReadModels.Vaults;
using VaultApi.Common.ReadModels.Wallets;

namespace VaultApi.Common.Persistence
{
    public class DatabaseContext : DbContext
    {
        public static string SchemaName { get; } = "vault_api";

        public static string MigrationHistoryTable { get; } = "__EFMigrationsHistory";

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<Blockchain> Blockchains { get; set; }

        public DbSet<TransactionSigningRequest> TransactionSigningRequests { get; set; }

        public DbSet<Vault> Vaults { get; set; }

        public DbSet<WalletGenerationRequest> WalletGenerationRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SchemaName);

            BuildBlockchain(modelBuilder);
            BuildTransactions(modelBuilder);
            BuildVaults(modelBuilder);
            BuildWalletGenerationRequests(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private static void BuildBlockchain(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blockchain>()
                .ToTable("blockchains")
                .HasKey(entity => entity.Id);

            modelBuilder.Entity<Blockchain>()
                .OwnsOne(entity => entity.Protocol,
                    action =>
                    {
                        action.Property(property => property.Code)
                            .HasColumnName("ProtocolCode");
                        action.Property(property => property.Name)
                            .HasColumnName("ProtocolName");
                        action.Property(property => property.DoubleSpendingProtectionType)
                            .HasColumnName("DoubleSpendingProtectionType");
                    });

            modelBuilder.Entity<Blockchain>()
                .HasIndex(entity => entity.TenantId);
        }

        private static void BuildTransactions(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransactionSigningRequest>()
                .ToTable("transaction_signing_requests")
                .HasKey(entity => entity.Id);

            var jsonSerializingSettings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};

            modelBuilder.Entity<TransactionSigningRequest>()
                .Property(e => e.SigningAddresses)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, jsonSerializingSettings),
                    v => JsonConvert.DeserializeObject<IReadOnlyCollection<string>>(v, jsonSerializingSettings));

            modelBuilder.Entity<TransactionSigningRequest>()
                .Property(e => e.CoinsToSpend)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, jsonSerializingSettings),
                    v => JsonConvert.DeserializeObject<IReadOnlyCollection<Coin>>(v, jsonSerializingSettings));

            modelBuilder.Entity<TransactionSigningRequest>()
                .HasIndex(entity => entity.TenantId);

            modelBuilder.Entity<TransactionSigningRequest>()
                .HasIndex(entity => entity.VaultId);

            modelBuilder.Entity<TransactionSigningRequest>()
                .HasIndex(entity => entity.VaultType);

            modelBuilder.Entity<TransactionSigningRequest>()
                .HasIndex(entity => entity.State);
        }

        private static void BuildVaults(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vault>()
                .ToTable("vaults")
                .HasKey(entity => entity.Id);

            modelBuilder.Entity<Vault>()
                .HasIndex(entity => entity.TenantId);
        }

        private static void BuildWalletGenerationRequests(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WalletGenerationRequest>()
                .ToTable("wallet_generation_requests")
                .HasKey(entity => entity.Id);

            modelBuilder.Entity<WalletGenerationRequest>()
                .HasIndex(entity => entity.TenantId);

            modelBuilder.Entity<WalletGenerationRequest>()
                .HasIndex(entity => entity.VaultId);

            modelBuilder.Entity<WalletGenerationRequest>()
                .HasIndex(entity => entity.VaultType);

            modelBuilder.Entity<WalletGenerationRequest>()
                .HasIndex(entity => entity.State);
        }
    }
}
