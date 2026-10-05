using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Black_order.Migrations
{
    /// <inheritdoc />
    public partial class categoriasName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                table: "Categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
