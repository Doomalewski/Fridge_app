using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fridge_app.Migrations
{
    /// <inheritdoc />
    public partial class migracjalisty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "ShoppingLists",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "ShoppingLists",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAddedToFridge",
                table: "ShoppingListItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "ShoppingLists");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "ShoppingLists");

            migrationBuilder.DropColumn(
                name: "IsAddedToFridge",
                table: "ShoppingListItems");
        }
    }
}
