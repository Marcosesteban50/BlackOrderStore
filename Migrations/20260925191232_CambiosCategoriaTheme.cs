using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Black_order.Migrations
{
    /// <inheritdoc />
    public partial class CambiosCategoriaTheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThemeKey",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThemeKey",
                table: "Categories");
        }
    }
}
