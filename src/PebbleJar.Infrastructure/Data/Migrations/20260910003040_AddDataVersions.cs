using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PebbleJar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDataVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "data_versions",
                columns: table => new
                {
                    scope = table.Column<int>(type: "INTEGER", nullable: false),
                    revision = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_versions", x => x.scope);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_versions");
        }
    }
}
