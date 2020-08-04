using Microsoft.EntityFrameworkCore.Migrations;

namespace VaultApi.Common.Migrations
{
    public partial class IndicesImproved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_wallet_generation_requests_VaultType_State",
                schema: "vault_api",
                table: "wallet_generation_requests");

            migrationBuilder.DropIndex(
                name: "IX_wallet_generation_requests_VaultId_VaultType_State",
                schema: "vault_api",
                table: "wallet_generation_requests");

            migrationBuilder.DropIndex(
                name: "IX_transaction_signing_requests_VaultType_State",
                schema: "vault_api",
                table: "transaction_signing_requests");

            migrationBuilder.DropIndex(
                name: "IX_transaction_signing_requests_VaultId_VaultType_State",
                schema: "vault_api",
                table: "transaction_signing_requests");

            migrationBuilder.DropColumn(
                name: "StartBlockNumber",
                schema: "vault_api",
                table: "blockchains");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_generation_requests_State",
                schema: "vault_api",
                table: "wallet_generation_requests",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_generation_requests_VaultId",
                schema: "vault_api",
                table: "wallet_generation_requests",
                column: "VaultId");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_generation_requests_VaultType",
                schema: "vault_api",
                table: "wallet_generation_requests",
                column: "VaultType");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_wallet_generation_requests_State",
                schema: "vault_api",
                table: "wallet_generation_requests");

            migrationBuilder.DropIndex(
                name: "IX_wallet_generation_requests_VaultId",
                schema: "vault_api",
                table: "wallet_generation_requests");

            migrationBuilder.DropIndex(
                name: "IX_wallet_generation_requests_VaultType",
                schema: "vault_api",
                table: "wallet_generation_requests");

            migrationBuilder.DropIndex(
                name: "IX_transaction_signing_requests_State",
                schema: "vault_api",
                table: "transaction_signing_requests");

            migrationBuilder.DropIndex(
                name: "IX_transaction_signing_requests_TenantId",
                schema: "vault_api",
                table: "transaction_signing_requests");

            migrationBuilder.DropIndex(
                name: "IX_transaction_signing_requests_VaultId",
                schema: "vault_api",
                table: "transaction_signing_requests");

            migrationBuilder.DropIndex(
                name: "IX_transaction_signing_requests_VaultType",
                schema: "vault_api",
                table: "transaction_signing_requests");

            migrationBuilder.AddColumn<long>(
                name: "StartBlockNumber",
                schema: "vault_api",
                table: "blockchains",
                type: "bigint",
                nullable: true);

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
        }
    }
}
