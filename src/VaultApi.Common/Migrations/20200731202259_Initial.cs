using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace VaultApi.Common.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "vault_api");

            migrationBuilder.CreateTable(
                name: "blockchains",
                schema: "vault_api",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    TenantId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    ProtocolCode = table.Column<string>(nullable: true),
                    ProtocolName = table.Column<string>(nullable: true),
                    StartBlockNumber = table.Column<long>(nullable: true),
                    DoubleSpendingProtectionType = table.Column<int>(nullable: true),
                    NetworkType = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blockchains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "transaction_signing_requests",
                schema: "vault_api",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<string>(nullable: true),
                    VaultId = table.Column<long>(nullable: false),
                    VaultType = table.Column<int>(nullable: false),
                    Component = table.Column<string>(nullable: true),
                    OperationId = table.Column<long>(nullable: false),
                    OperationType = table.Column<string>(nullable: true),
                    BlockchainId = table.Column<string>(nullable: true),
                    State = table.Column<int>(nullable: false),
                    RejectionReasonMessage = table.Column<string>(nullable: true),
                    RejectionReason = table.Column<int>(nullable: true),
                    NetworkType = table.Column<int>(nullable: false),
                    ProtocolCode = table.Column<string>(nullable: true),
                    DoubleSpendingProtectionType = table.Column<int>(nullable: false),
                    BuiltTransaction = table.Column<byte[]>(nullable: true),
                    SigningAddresses = table.Column<string>(nullable: true),
                    CoinsToSpend = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_signing_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vaults",
                schema: "vault_api",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    TenantId = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vaults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "wallet_generation_requests",
                schema: "vault_api",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<string>(nullable: true),
                    VaultId = table.Column<long>(nullable: false),
                    VaultType = table.Column<int>(nullable: false),
                    BlockchainId = table.Column<string>(nullable: true),
                    NetworkType = table.Column<int>(nullable: false),
                    ProtocolCode = table.Column<string>(nullable: true),
                    Component = table.Column<string>(nullable: true),
                    State = table.Column<int>(nullable: false),
                    RejectionReason = table.Column<int>(nullable: true),
                    RejectionReasonMessage = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallet_generation_requests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_blockchains_TenantId",
                schema: "vault_api",
                table: "blockchains",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_signing_requests_VaultType_State",
                schema: "vault_api",
                table: "transaction_signing_requests",
                columns: new[] { "VaultType", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_transaction_signing_requests_VaultId_VaultType_State",
                schema: "vault_api",
                table: "transaction_signing_requests",
                columns: new[] { "VaultId", "VaultType", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_vaults_TenantId",
                schema: "vault_api",
                table: "vaults",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_generation_requests_TenantId",
                schema: "vault_api",
                table: "wallet_generation_requests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_generation_requests_VaultType_State",
                schema: "vault_api",
                table: "wallet_generation_requests",
                columns: new[] { "VaultType", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_wallet_generation_requests_VaultId_VaultType_State",
                schema: "vault_api",
                table: "wallet_generation_requests",
                columns: new[] { "VaultId", "VaultType", "State" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blockchains",
                schema: "vault_api");

            migrationBuilder.DropTable(
                name: "transaction_signing_requests",
                schema: "vault_api");

            migrationBuilder.DropTable(
                name: "vaults",
                schema: "vault_api");

            migrationBuilder.DropTable(
                name: "wallet_generation_requests",
                schema: "vault_api");
        }
    }
}
