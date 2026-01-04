using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fridge_app.Migrations
{
    /// <inheritdoc />
    public partial class AddDietToHumanStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Diet",
                table: "HumanStats",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Diet",
                table: "HumanStats");
        }
    }
}
