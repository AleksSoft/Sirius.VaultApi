using Microsoft.EntityFrameworkCore.Migrations;

namespace VaultApi.Common.Migrations
{
    public partial class TransferSigningRequestGroupUpdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Group",
                schema: "vault_api",
                table: "transaction_signing_requests",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Group",
                schema: "vault_api",
                table: "transaction_signing_requests");
        }
    }
}
