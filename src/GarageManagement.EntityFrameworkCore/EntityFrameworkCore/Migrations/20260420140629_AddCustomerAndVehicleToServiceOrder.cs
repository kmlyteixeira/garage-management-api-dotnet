using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagement.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerAndVehicleToServiceOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "AppServiceOrders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "VehicleId",
                table: "AppServiceOrders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AppServiceOrders_EstimateId",
                table: "AppServiceOrders",
                column: "EstimateId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppServiceOrders_AppEstimates_EstimateId",
                table: "AppServiceOrders",
                column: "EstimateId",
                principalTable: "AppEstimates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppServiceOrders_AppEstimates_EstimateId",
                table: "AppServiceOrders");

            migrationBuilder.DropIndex(
                name: "IX_AppServiceOrders_EstimateId",
                table: "AppServiceOrders");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "AppServiceOrders");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "AppServiceOrders");
        }
    }
}
