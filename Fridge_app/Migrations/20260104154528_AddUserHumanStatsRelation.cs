using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fridge_app.Migrations
{
    /// <inheritdoc />
    public partial class AddUserHumanStatsRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HumanStatsId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_HumanStatsId",
                table: "Users",
                column: "HumanStatsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_HumanStats_HumanStatsId",
                table: "Users",
                column: "HumanStatsId",
                principalTable: "HumanStats",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_HumanStats_HumanStatsId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_HumanStatsId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HumanStatsId",
                table: "Users");
        }
    }
}
