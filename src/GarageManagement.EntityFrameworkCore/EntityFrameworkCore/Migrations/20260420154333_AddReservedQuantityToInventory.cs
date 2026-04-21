using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagement.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddReservedQuantityToInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReservedQuantity",
                table: "AppInventories",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservedQuantity",
                table: "AppInventories");
        }
    }
}
