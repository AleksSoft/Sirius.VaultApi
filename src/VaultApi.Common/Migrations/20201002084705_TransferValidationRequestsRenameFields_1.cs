using Microsoft.EntityFrameworkCore.Migrations;

namespace VaultApi.Common.Migrations
{
    public partial class TransferValidationRequestsRenameFields_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReasonString",
                schema: "vault_api",
                table: "transfer_validation_requests");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReasonMessage",
                schema: "vault_api",
                table: "transfer_validation_requests",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReasonMessage",
                schema: "vault_api",
                table: "transfer_validation_requests");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReasonString",
                schema: "vault_api",
                table: "transfer_validation_requests",
                type: "text",
                nullable: true);
        }
    }
}
