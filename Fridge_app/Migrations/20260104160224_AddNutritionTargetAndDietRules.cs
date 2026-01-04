using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fridge_app.Migrations
{
    /// <inheritdoc />
    public partial class AddNutritionTargetAndDietRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Diets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RulesJson",
                table: "Diets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DietProductRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DietId = table.Column<int>(type: "integer", nullable: false),
                    ProductCategory = table.Column<string>(type: "text", nullable: false),
                    IsAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "NutritionTargets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CaloriesPerDay = table.Column<int>(type: "integer", nullable: false),
                    ProteinGrams = table.Column<double>(type: "double precision", nullable: false),
                    FatGrams = table.Column<double>(type: "double precision", nullable: false),
                    CarbsGrams = table.Column<double>(type: "double precision", nullable: false),
                    Goal = table.Column<string>(type: "text", nullable: false),
                    DietId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NutritionTargets_Diets_DietId",
                        column: x => x.DietId,
                        principalTable: "Diets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NutritionTargets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NutritionTargets_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DietProductRules_DietId_ProductCategory",
                table: "DietProductRules",
                columns: new[] { "DietId", "ProductCategory" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NutritionTargets_DietId",
                table: "NutritionTargets",
                column: "DietId");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionTargets_UserId",
                table: "NutritionTargets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionTargets_UserId1",
                table: "NutritionTargets",
                column: "UserId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DietProductRules");

            migrationBuilder.DropTable(
                name: "NutritionTargets");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Diets");

            migrationBuilder.DropColumn(
                name: "RulesJson",
                table: "Diets");
        }
    }
}
