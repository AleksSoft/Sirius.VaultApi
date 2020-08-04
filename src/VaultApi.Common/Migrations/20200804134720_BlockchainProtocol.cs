using Microsoft.EntityFrameworkCore.Migrations;

namespace VaultApi.Common.Migrations
{
    public partial class BlockchainProtocol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_blockchains_TenantId",
                schema: "vault_api",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "ProtocolCode",
                schema: "vault_api",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "DoubleSpendingProtectionType",
                schema: "vault_api",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "ProtocolName",
                schema: "vault_api",
                table: "blockchains");

            migrationBuilder.AddColumn<string>(
                name: "Protocol",
                schema: "vault_api",
                table: "blockchains",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Protocol",
                schema: "vault_api",
                table: "blockchains");

            migrationBuilder.AddColumn<string>(
                name: "ProtocolCode",
                schema: "vault_api",
                table: "blockchains",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DoubleSpendingProtectionType",
                schema: "vault_api",
                table: "blockchains",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProtocolName",
                schema: "vault_api",
                table: "blockchains",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_blockchains_TenantId",
                schema: "vault_api",
                table: "blockchains",
                column: "TenantId");
        }
    }
}
