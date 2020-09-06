using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace VaultApi.Common.Migrations
{
    public partial class TransactionApproval : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "key_keepers",
                schema: "vault_api",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<string>(nullable: true),
                    KeyId = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_key_keepers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "transaction_approval_confirmations",
                schema: "vault_api",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<string>(nullable: true),
                    TransactionApprovalRequestId = table.Column<long>(nullable: false),
                    KeyId = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    Secret = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_approval_confirmations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_key_keepers_KeyId",
                schema: "vault_api",
                table: "key_keepers",
                column: "KeyId");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_approval_confirmations_TransactionApprovalReque~",
                schema: "vault_api",
                table: "transaction_approval_confirmations",
                column: "TransactionApprovalRequestId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "key_keepers",
                schema: "vault_api");

            migrationBuilder.DropTable(
                name: "transaction_approval_confirmations",
                schema: "vault_api");
        }
    }
}
