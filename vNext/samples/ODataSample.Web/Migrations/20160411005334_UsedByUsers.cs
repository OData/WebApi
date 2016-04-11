using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ODataSample.Web.Migrations
{
    public partial class UsedByUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsedProductId",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UsedProductId",
                table: "AspNetUsers",
                column: "UsedProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Products_UsedProductId",
                table: "AspNetUsers",
                column: "UsedProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Products_UsedProductId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_UsedProductId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UsedProductId",
                table: "AspNetUsers");
        }
    }
}
