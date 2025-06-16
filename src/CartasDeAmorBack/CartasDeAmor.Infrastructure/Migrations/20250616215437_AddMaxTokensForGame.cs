using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CartasDeAmor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxTokensForGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameStarted",
                table: "Games");

            migrationBuilder.AddColumn<int>(
                name: "MaxTokens",
                table: "Games",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxTokens",
                table: "Games");

            migrationBuilder.AddColumn<bool>(
                name: "GameStarted",
                table: "Games",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
