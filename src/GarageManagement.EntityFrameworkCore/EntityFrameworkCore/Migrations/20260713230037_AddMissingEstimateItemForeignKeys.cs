using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagement.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingEstimateItemForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppEstimateServiceItems_ServiceId",
                table: "AppEstimateServiceItems",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_AppEstimateProductItems_ProductId",
                table: "AppEstimateProductItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppEstimateProductItems_AppProducts_ProductId",
                table: "AppEstimateProductItems",
                column: "ProductId",
                principalTable: "AppProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppEstimateServiceItems_AppServices_ServiceId",
                table: "AppEstimateServiceItems",
                column: "ServiceId",
                principalTable: "AppServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppEstimateProductItems_AppProducts_ProductId",
                table: "AppEstimateProductItems");

            migrationBuilder.DropForeignKey(
                name: "FK_AppEstimateServiceItems_AppServices_ServiceId",
                table: "AppEstimateServiceItems");

            migrationBuilder.DropIndex(
                name: "IX_AppEstimateServiceItems_ServiceId",
                table: "AppEstimateServiceItems");

            migrationBuilder.DropIndex(
                name: "IX_AppEstimateProductItems_ProductId",
                table: "AppEstimateProductItems");
        }
    }
}
