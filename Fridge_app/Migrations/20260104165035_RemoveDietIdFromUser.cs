using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fridge_app.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDietIdFromUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Diets_DietId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_DietId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DietId",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DietId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DietId",
                table: "Users",
                column: "DietId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Diets_DietId",
                table: "Users",
                column: "DietId",
                principalTable: "Diets",
                principalColumn: "Id");
        }
    }
}
