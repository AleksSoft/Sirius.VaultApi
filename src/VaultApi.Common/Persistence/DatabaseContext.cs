﻿using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using VaultApi.Common.Migrations;
using VaultApi.Common.ReadModels.Blockchains;
using VaultApi.Common.ReadModels.KeyKeepers;
using VaultApi.Common.ReadModels.TransactionApprovalConfirmations;
using VaultApi.Common.ReadModels.TransferSigningRequests;
using VaultApi.Common.ReadModels.TransferValidationRequests;
using VaultApi.Common.ReadModels.Vaults;
using VaultApi.Common.ReadModels.Wallets;

namespace VaultApi.Common.Persistence
{
    public class DatabaseContext : DbContext
    {
        private static readonly JsonSerializerSettings JsonSerializingSettings =
            new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};

        public static string SchemaName { get; } = "vault_api";

        public static string MigrationHistoryTable { get; } = "__EFMigrationsHistory";

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<VaultApi.Common.ReadModels.Blockchains.Blockchain> Blockchains { get; set; }

        public DbSet<KeyKeeper> KeyKeepers { get; set; }

        public DbSet<TransactionApprovalConfirmation> TransactionApprovalConfirmations { get; set; }

        public DbSet<TransferSigningRequest> TransferSigningRequests { get; set; }

        public DbSet<Vault> Vaults { get; set; }

        public DbSet<WalletGenerationRequest> WalletGenerationRequests { get; set; }

        public DbSet<TransferValidationRequest> TransferValidationRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SchemaName);

            BuildBlockchain(modelBuilder);
            BuildKeyKeepers(modelBuilder);
            BuildTransactionApprovalConfirmations(modelBuilder);
            BuildVaults(modelBuilder);
            BuildWalletGenerationRequests(modelBuilder);
            BuildTransferSigningRequests(modelBuilder);
            BuildTransferValidationRequests(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private static void BuildBlockchain(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VaultApi.Common.ReadModels.Blockchains.Blockchain>()
                .ToTable("blockchains")
                .HasKey(entity => entity.Id);

            modelBuilder.Entity<VaultApi.Common.ReadModels.Blockchains.Blockchain>()
                .Property(e => e.Protocol)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, JsonSerializingSettings),
                    v => JsonConvert.DeserializeObject<Protocol>(v, JsonSerializingSettings));
        }

        private static void BuildKeyKeepers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KeyKeeper>()
                .ToTable("key_keepers")
                .HasKey(property => property.Id);

            modelBuilder.Entity<KeyKeeper>()
                .HasIndex(property => property.KeyId);
        }

        private static void BuildTransactionApprovalConfirmations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransactionApprovalConfirmation>()
                .ToTable("transaction_approval_confirmations")
                .HasKey(entity => entity.Id);

            modelBuilder.Entity<TransactionApprovalConfirmation>()
                .HasIndex(entity => entity.TransactionApprovalRequestId);
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

            modelBuilder.Entity<WalletGenerationRequest>()
                .Property(entity => entity.Group)
                .IsRequired();

            modelBuilder.Entity<WalletGenerationRequest>()
                .Property(e => e.WalletGenerationContext)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, JsonSerializingSettings),
                    v => JsonConvert.DeserializeObject<WalletGenerationContext>(v,
                        JsonSerializingSettings));
        }

        private static void BuildTransferSigningRequests(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransferSigningRequest>()
                .ToTable("transfer_signing_requests")
                .HasKey(entity => entity.Id);

            modelBuilder.Entity<TransferSigningRequest>()
                .Property(e => e.Blockchain)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, JsonSerializingSettings),
                    v => JsonConvert.DeserializeObject<ReadModels.TransferSigningRequests.Blockchain>(v,
                        JsonSerializingSettings));

            modelBuilder.Entity<TransferSigningRequest>()
                .Property(e => e.SigningAddresses)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, JsonSerializingSettings),
                    v => JsonConvert.DeserializeObject<IReadOnlyCollection<string>>(v, JsonSerializingSettings));

            modelBuilder.Entity<TransferSigningRequest>()
                .Property(e => e.CoinsToSpend)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, JsonSerializingSettings),
                    v => JsonConvert.DeserializeObject<IReadOnlyCollection<Coin>>(v, JsonSerializingSettings));

            modelBuilder.Entity<TransferSigningRequest>()
                .HasIndex(entity => entity.TenantId);

            modelBuilder.Entity<TransferSigningRequest>()
                .HasIndex(entity => entity.VaultId);

            modelBuilder.Entity<TransferSigningRequest>()
                .HasIndex(entity => entity.VaultType);

            modelBuilder.Entity<TransferSigningRequest>()
                .HasIndex(entity => entity.State);
        }

        private void BuildTransferValidationRequests(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransferValidationRequest>()
                .ToTable("transfer_validation_requests")
                .HasKey(x => x.Id);

            modelBuilder.Entity<TransferValidationRequest>()
                .HasIndex(entity => entity.TenantId);

            modelBuilder.Entity<TransferValidationRequest>()
                .HasIndex(entity => entity.VaultId);

            modelBuilder.Entity<TransferValidationRequest>()
                .HasIndex(entity => entity.VaultType);

            modelBuilder.Entity<TransferValidationRequest>()
                .HasIndex(entity => entity.State);

            modelBuilder.Entity<TransferValidationRequest>()
                .Property(e => e.Blockchain)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v,
                        JsonSerializingSettings),
                    v =>
                        JsonConvert.DeserializeObject<ReadModels.TransferValidationRequests.Blockchain>(v,
                            JsonSerializingSettings));

            modelBuilder.Entity<TransferValidationRequest>()
                .Property(e => e.Asset)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v,
                        JsonSerializingSettings),
                    v =>
                        JsonConvert.DeserializeObject<Asset>(v,
                            JsonSerializingSettings));

            modelBuilder.Entity<TransferValidationRequest>()
                .Property(e => e.SourceAddress)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v,
                        JsonSerializingSettings),
                    v =>
                        JsonConvert.DeserializeObject<SourceAddress>(v,
                            JsonSerializingSettings));

            modelBuilder.Entity<TransferValidationRequest>()
                .Property(e => e.DestinationAddress)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v,
                        JsonSerializingSettings),
                    v =>
                        JsonConvert.DeserializeObject<DestinationAddress>(v,
                            JsonSerializingSettings));

            modelBuilder.Entity<TransferValidationRequest>()
                .Property(e => e.TransferContext)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v,
                        JsonSerializingSettings),
                    v =>
                        JsonConvert.DeserializeObject<ReadModels.TransferValidationRequests.TransferContext>(v,
                            JsonSerializingSettings));
        }
    }
}
