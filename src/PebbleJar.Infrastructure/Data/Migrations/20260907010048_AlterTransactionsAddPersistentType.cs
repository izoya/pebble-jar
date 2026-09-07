using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PebbleJar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterTransactionsAddPersistentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "transactions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE transactions
                SET type = CASE
                    WHEN CAST(amount AS REAL) < 0 THEN 1
                    ELSE 2
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "transactions");
        }
    }
}
