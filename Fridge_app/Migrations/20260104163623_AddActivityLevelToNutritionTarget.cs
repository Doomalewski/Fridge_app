using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fridge_app.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityLevelToNutritionTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActivityLevel",
                table: "NutritionTargets",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivityLevel",
                table: "NutritionTargets");
        }
    }
}
