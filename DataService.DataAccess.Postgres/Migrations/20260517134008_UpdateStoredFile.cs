using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataService.DataAccess.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStoredFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "StoredFiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "StoredFiles");
        }
    }
}
