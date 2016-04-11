using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ODataSample.Web.Migrations
{
    public partial class ApplicationRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FavouriteProductId",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_FavouriteProductId",
                table: "AspNetUsers",
                column: "FavouriteProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Products_FavouriteProductId",
                table: "AspNetUsers",
                column: "FavouriteProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Products_FavouriteProductId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_FavouriteProductId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FavouriteProductId",
                table: "AspNetUsers");
        }
    }
}
