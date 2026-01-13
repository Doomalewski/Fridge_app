using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fridge_app.Migrations
{
    /// <inheritdoc />
    public partial class AddShoppingLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meals_Diets_DietId",
                table: "Meals");

            migrationBuilder.DropForeignKey(
                name: "FK_NutritionTargets_Diets_DietId",
                table: "NutritionTargets");

            migrationBuilder.DropTable(
                name: "DietProductRules");

            migrationBuilder.DropTable(
                name: "Diets");

            migrationBuilder.DropIndex(
                name: "IX_NutritionTargets_DietId",
                table: "NutritionTargets");

            migrationBuilder.DropIndex(
                name: "IX_Meals_DietId",
                table: "Meals");

            migrationBuilder.DropColumn(
                name: "DietId",
                table: "NutritionTargets");

            migrationBuilder.DropColumn(
                name: "DietId",
                table: "Meals");

            migrationBuilder.CreateTable(
                name: "ShoppingLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Budget = table.Column<decimal>(type: "numeric", nullable: true),
                    Cuisine = table.Column<string>(type: "text", nullable: true),
                    DesiredDish = table.Column<string>(type: "text", nullable: true),
                    AdditionalNotes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoppingLists_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingListItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShoppingListId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Quantity = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Category = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoppingListItems_ShoppingLists_ShoppingListId",
                        column: x => x.ShoppingListId,
                        principalTable: "ShoppingLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingListItems_ShoppingListId",
                table: "ShoppingListItems",
                column: "ShoppingListId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingLists_UserId",
                table: "ShoppingLists",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShoppingListItems");

            migrationBuilder.DropTable(
                name: "ShoppingLists");

            migrationBuilder.AddColumn<int>(
                name: "DietId",
                table: "NutritionTargets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DietId",
                table: "Meals",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Diets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Calories = table.Column<int>(type: "integer", nullable: false),
                    DietType = table.Column<string>(type: "text", nullable: false),
                    Goal = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RulesJson = table.Column<string>(type: "text", nullable: false),
                    TimeSpan = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DietProductRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DietId = table.Column<int>(type: "integer", nullable: false),
                    IsAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    ProductCategory = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DietProductRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DietProductRules_Diets_DietId",
                        column: x => x.DietId,
                        principalTable: "Diets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NutritionTargets_DietId",
                table: "NutritionTargets",
                column: "DietId");

            migrationBuilder.CreateIndex(
                name: "IX_Meals_DietId",
                table: "Meals",
                column: "DietId");

            migrationBuilder.CreateIndex(
                name: "IX_DietProductRules_DietId_ProductCategory",
                table: "DietProductRules",
                columns: new[] { "DietId", "ProductCategory" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Meals_Diets_DietId",
                table: "Meals",
                column: "DietId",
                principalTable: "Diets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NutritionTargets_Diets_DietId",
                table: "NutritionTargets",
                column: "DietId",
                principalTable: "Diets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
