using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PebbleJar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterFinancialInstitutionsAddExternalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_financial_institutions_name",
                table: "financial_institutions");

            migrationBuilder.AddColumn<int>(
                name: "connection_provider",
                table: "financial_institutions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "financial_institutions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_financial_institutions_external_id_name",
                table: "financial_institutions",
                columns: new[] { "external_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_financial_institutions_external_id_name",
                table: "financial_institutions");

            migrationBuilder.DropColumn(
                name: "connection_provider",
                table: "financial_institutions");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "financial_institutions");

            migrationBuilder.CreateIndex(
                name: "ix_financial_institutions_name",
                table: "financial_institutions",
                column: "name",
                unique: true);
        }
    }
}
