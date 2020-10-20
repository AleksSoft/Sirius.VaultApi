using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace VaultApi.Common.Migrations
{
    public partial class TransferContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transaction_signing_requests",
                schema: "vault_api");

            migrationBuilder.DropColumn(
                name: "CustomerSignature",
                schema: "vault_api",
                table: "transfer_validation_requests");

            migrationBuilder.DropColumn(
                name: "Details",
                schema: "vault_api",
                table: "transfer_validation_requests");

            migrationBuilder.DropColumn(
                name: "SiriusSignature",
                schema: "vault_api",
                table: "transfer_validation_requests");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                schema: "vault_api",
                table: "transfer_validation_requests",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Asset",
                schema: "vault_api",
                table: "transfer_validation_requests",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Blockchain",
                schema: "vault_api",
                table: "transfer_validation_requests",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationAddress",
                schema: "vault_api",
                table: "transfer_validation_requests",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Document",
                schema: "vault_api",
                table: "transfer_validation_requests",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeLimit",
                schema: "vault_api",
                table: "transfer_validation_requests",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Signature",
                schema: "vault_api",
                table: "transfer_validation_requests",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceAddress",
                schema: "vault_api",
                table: "transfer_validation_requests",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransferContext",
                schema: "vault_api",
                table: "transfer_validation_requests",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TransferId",
                schema: "vault_api",
                table: "transfer_validation_requests",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "transfer_signing_requests",
                schema: "vault_api",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransferId = table.Column<long>(nullable: false),
                    TenantId = table.Column<string>(nullable: true),
                    VaultId = table.Column<long>(nullable: false),
                    VaultType = table.Column<int>(nullable: false),
                    Blockchain = table.Column<string>(nullable: true),
                    BuiltTransaction = table.Column<byte[]>(nullable: true),
                    SigningAddresses = table.Column<string>(nullable: true),
                    CoinsToSpend = table.Column<string>(nullable: true),
                    RejectionReason = table.Column<int>(nullable: true),
                    RejectionReasonMessage = table.Column<string>(nullable: true),
                    State = table.Column<int>(nullable: false),
                    Document = table.Column<string>(nullable: true),
                    Signature = table.Column<string>(nullable: true),
                    Group = table.Column<string>(nullable: true),
                    Sequence = table.Column<long>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transfer_signing_requests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_transfer_signing_requests_State",
                schema: "vault_api",
                table: "transfer_signing_requests",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_transfer_signing_requests_TenantId",
                schema: "vault_api",
                table: "transfer_signing_requests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_transfer_signing_requests_VaultId",
                schema: "vault_api",
                table: "transfer_signing_requests",
                column: "VaultId");

            migrationBuilder.CreateIndex(
                name: "IX_transfer_signing_requests_VaultType",
                schema: "vault_api",
                table: "transfer_signing_requests",
                column: "VaultType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transfer_signing_requests",
                schema: "vault_api");

            migrationBuilder.DropColumn(
                name: "Amount",
                schema: "vault_api",
                table: "transfer_validation_requests");

            migrationBuilder.DropColumn(
                name: "Asset",
                schema: "vault_api",
                table: "transfer_validation_requests");

            migrationBuilder.DropColumn(
                name: "Blockchain",
                schema: "vault_api",
                table: "transfer_validation_requests");

            migrationBuilder.DropColumn(
                name: "DestinationAddress",
                schema: "vault_api",
                table: "transfer_validation_requests");

            migrationBuilder.DropColumn(
                name: "Document",
                schema: "vault_api",
                table: "transfer_validation_requests");

            migrationBuilder.DropColumn(
                name: "FeeLimit",
                schema: "vault_api",
                table: "transfer_validation_requests");

            migrationBuilder.DropColumn(
                name: "Signature",
                schema: "vault_api",
                table: "transfer_validation_requests");

            migrationBuilder.DropColumn(
                name: "SourceAddress",
                schema: "vault_api",
                table: "transfer_validation_requests");

            migrationBuilder.DropColumn(
                name: "TransferContext",
                schema: "vault_api",
                table: "transfer_validation_requests");

            migrationBuilder.DropColumn(
                name: "TransferId",
                schema: "vault_api",
                table: "transfer_validation_requests");

            migrationBuilder.AddColumn<string>(
                name: "CustomerSignature",
                schema: "vault_api",
                table: "transfer_validation_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Details",
                schema: "vault_api",
                table: "transfer_validation_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiriusSignature",
                schema: "vault_api",
                table: "transfer_validation_requests",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "transaction_signing_requests",
                schema: "vault_api",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BlockchainId = table.Column<string>(type: "text", nullable: true),
                    BuiltTransaction = table.Column<byte[]>(type: "bytea", nullable: true),
                    CoinsToSpend = table.Column<string>(type: "text", nullable: true),
                    Component = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DoubleSpendingProtectionType = table.Column<int>(type: "integer", nullable: false),
                    Group = table.Column<string>(type: "text", nullable: true),
                    NetworkType = table.Column<int>(type: "integer", nullable: false),
                    OperationId = table.Column<long>(type: "bigint", nullable: false),
                    OperationType = table.Column<string>(type: "text", nullable: true),
                    ProtocolCode = table.Column<string>(type: "text", nullable: true),
                    RejectionReason = table.Column<int>(type: "integer", nullable: true),
                    RejectionReasonMessage = table.Column<string>(type: "text", nullable: true),
                    SigningAddresses = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserContext = table.Column<string>(type: "text", nullable: true),
                    VaultId = table.Column<long>(type: "bigint", nullable: false),
                    VaultType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_signing_requests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_transaction_signing_requests_State",
                schema: "vault_api",
                table: "transaction_signing_requests",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_signing_requests_TenantId",
                schema: "vault_api",
                table: "transaction_signing_requests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_signing_requests_VaultId",
                schema: "vault_api",
                table: "transaction_signing_requests",
                column: "VaultId");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_signing_requests_VaultType",
                schema: "vault_api",
                table: "transaction_signing_requests",
                column: "VaultType");
        }
    }
}
