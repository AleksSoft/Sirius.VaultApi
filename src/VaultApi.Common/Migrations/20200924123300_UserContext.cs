using Microsoft.EntityFrameworkCore.Migrations;

namespace VaultApi.Common.Migrations
{
    public partial class UserContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserContext",
                schema: "vault_api",
                table: "transaction_signing_requests",
                nullable: true);

            migrationBuilder.Sql(@"
                            UPDATE vault_api.transaction_signing_requests 
                            SET ""UserContext"" = '{}' 
                            WHERE ""UserContext"" is null;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserContext",
                schema: "vault_api",
                table: "transaction_signing_requests");
        }
    }
}
