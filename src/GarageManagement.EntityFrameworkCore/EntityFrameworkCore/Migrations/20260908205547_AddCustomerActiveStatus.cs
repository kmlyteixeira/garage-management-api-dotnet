using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagement.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerActiveStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AppCustomers",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AppCustomers");
        }
    }
}
