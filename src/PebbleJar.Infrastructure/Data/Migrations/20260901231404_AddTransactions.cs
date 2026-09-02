using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PebbleJar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    external_id = table.Column<string>(type: "TEXT", nullable: true),
                    account_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    external_account_id = table.Column<string>(type: "TEXT", nullable: true),
                    transaction_date_time = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    category = table.Column<int>(type: "INTEGER", nullable: false),
                    kind = table.Column<int>(type: "INTEGER", nullable: false),
                    source_payload_json = table.Column<string>(type: "TEXT", nullable: true),
                    source_fetched_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    recognition_data = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_transactions_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_transactions_account_id_external_id",
                table: "transactions",
                columns: new[] { "account_id", "external_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transactions");
        }
    }
}
