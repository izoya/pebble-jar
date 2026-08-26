using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PebbleJar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountIsSyncEnabledColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_sync_enabled",
                table: "accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_sync_enabled",
                table: "accounts");
        }
    }
}
