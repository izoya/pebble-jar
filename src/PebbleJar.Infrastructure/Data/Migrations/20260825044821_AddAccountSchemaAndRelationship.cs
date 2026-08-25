using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PebbleJar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountSchemaAndRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    account_number = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    financial_institution_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    connection_provider = table.Column<int>(type: "INTEGER", nullable: true),
                    external_id = table.Column<string>(type: "TEXT", nullable: true),
                    status = table.Column<int>(type: "INTEGER", nullable: false),
                    currency = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    source_payload_json = table.Column<string>(type: "TEXT", nullable: true),
                    source_fetched_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_accounts_financial_institutions_financial_institution_id",
                        column: x => x.financial_institution_id,
                        principalTable: "financial_institutions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_accounts_connection_provider_external_id",
                table: "accounts",
                columns: new[] { "connection_provider", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_accounts_financial_institution_id",
                table: "accounts",
                column: "financial_institution_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accounts");
        }
    }
}
