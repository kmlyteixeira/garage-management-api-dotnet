using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageManagement.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class SyncEstimateModelWithCurrentEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AppEstimatePartItems",
                table: "AppEstimatePartItems");

            migrationBuilder.RenameTable(
                name: "AppEstimatePartItems",
                newName: "AppEstimateProductItems");

            migrationBuilder.RenameIndex(
                name: "IX_AppEstimatePartItems_EstimateId_ProductId",
                table: "AppEstimateProductItems",
                newName: "IX_AppEstimateProductItems_EstimateId_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppEstimateProductItems",
                table: "AppEstimateProductItems",
                column: "Id");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AppEstimateProductItems");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "AppEstimateProductItems");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AppEstimateServiceItems");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "AppEstimateServiceItems");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AppEstimateProductItems",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "AppEstimateProductItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AppEstimateServiceItems",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "AppEstimateServiceItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppEstimateProductItems",
                table: "AppEstimateProductItems");

            migrationBuilder.RenameTable(
                name: "AppEstimateProductItems",
                newName: "AppEstimatePartItems");

            migrationBuilder.RenameIndex(
                name: "IX_AppEstimateProductItems_EstimateId_ProductId",
                table: "AppEstimatePartItems",
                newName: "IX_AppEstimatePartItems_EstimateId_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppEstimatePartItems",
                table: "AppEstimatePartItems",
                column: "Id");
        }
    }
}
