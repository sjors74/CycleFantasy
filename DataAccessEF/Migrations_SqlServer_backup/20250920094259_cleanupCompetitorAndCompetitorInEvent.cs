using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class cleanupCompetitorAndCompetitorInEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompetitorId",
                table: "CompetitorsInEvent");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Competitors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompetitorId",
                table: "CompetitorsInEvent",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "Competitors",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
