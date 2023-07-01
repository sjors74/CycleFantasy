using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class teamnametogamecompetitorevent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamName",
                table: "GameCompetitors");

            migrationBuilder.AddColumn<string>(
                name: "TeamName",
                table: "GameCompetitorsEvent",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "GameCompetitorEventId",
                table: "GameCompetitorEventPicks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamName",
                table: "GameCompetitorsEvent");

            migrationBuilder.DropColumn(
                name: "GameCompetitorEventId",
                table: "GameCompetitorEventPicks");

            migrationBuilder.AddColumn<string>(
                name: "TeamName",
                table: "GameCompetitors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
