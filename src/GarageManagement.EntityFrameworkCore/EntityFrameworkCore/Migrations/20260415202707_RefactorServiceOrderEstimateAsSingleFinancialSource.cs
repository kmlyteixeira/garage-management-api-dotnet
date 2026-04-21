using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagement.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class RefactorServiceOrderEstimateAsSingleFinancialSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppServiceOrderPartItems");

            migrationBuilder.DropTable(
                name: "AppServiceOrderServiceItems");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "AppServiceOrders");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "AppServiceOrders");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "AppServiceOrders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "AppServiceOrders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "AppServiceOrders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "VehicleId",
                table: "AppServiceOrders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "AppServiceOrderPartItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    IsConsumed = table.Column<bool>(type: "boolean", nullable: false),
                    IsReserved = table.Column<bool>(type: "boolean", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    ServiceOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppServiceOrderPartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppServiceOrderPartItems_AppServiceOrders_ServiceOrderId",
                        column: x => x.ServiceOrderId,
                        principalTable: "AppServiceOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppServiceOrderServiceItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppServiceOrderServiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppServiceOrderServiceItems_AppServiceOrders_ServiceOrderId",
                        column: x => x.ServiceOrderId,
                        principalTable: "AppServiceOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppServiceOrderPartItems_ServiceOrderId_ProductId",
                table: "AppServiceOrderPartItems",
                columns: new[] { "ServiceOrderId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppServiceOrderServiceItems_ServiceOrderId_ServiceId",
                table: "AppServiceOrderServiceItems",
                columns: new[] { "ServiceOrderId", "ServiceId" });
        }
    }
}
