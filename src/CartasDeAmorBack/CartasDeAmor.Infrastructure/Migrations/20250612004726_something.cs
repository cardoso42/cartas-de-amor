using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CartasDeAmor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class something : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoldingCard",
                table: "Players");

            migrationBuilder.AddColumn<int[]>(
                name: "HoldingCards",
                table: "Players",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoldingCards",
                table: "Players");

            migrationBuilder.AddColumn<int>(
                name: "HoldingCard",
                table: "Players",
                type: "integer",
                nullable: true);
        }
    }
}
