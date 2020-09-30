using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace VaultApi.Common.Migrations
{
    public partial class TransferValidationRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transfer_validation_requests",
                schema: "vault_api",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Details = table.Column<string>(nullable: true),
                    CustomerSignature = table.Column<string>(nullable: true),
                    SiriusSignature = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    State = table.Column<int>(nullable: false),
                    Sequence = table.Column<long>(nullable: false),
                    RejectionReason = table.Column<int>(nullable: true),
                    RejectionReasonString = table.Column<string>(nullable: true),
                    VaultId = table.Column<long>(nullable: false),
                    VaultType = table.Column<int>(nullable: false),
                    TenantId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transfer_validation_requests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_transfer_validation_requests_State",
                schema: "vault_api",
                table: "transfer_validation_requests",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_transfer_validation_requests_TenantId",
                schema: "vault_api",
                table: "transfer_validation_requests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_transfer_validation_requests_VaultId",
                schema: "vault_api",
                table: "transfer_validation_requests",
                column: "VaultId");

            migrationBuilder.CreateIndex(
                name: "IX_transfer_validation_requests_VaultType",
                schema: "vault_api",
                table: "transfer_validation_requests",
                column: "VaultType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transfer_validation_requests",
                schema: "vault_api");
        }
    }
}
