using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class gamecompetitorsrestoreCompetitorInevent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitorsInEvent_GameCompetitorPicks_GameCompetitorEventPickId",
                table: "CompetitorsInEvent");

            migrationBuilder.DropIndex(
                name: "IX_CompetitorsInEvent_GameCompetitorEventPickId",
                table: "CompetitorsInEvent");

            migrationBuilder.DropColumn(
                name: "GameCompetitorEventPickId",
                table: "CompetitorsInEvent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameCompetitorEventPickId",
                table: "CompetitorsInEvent",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorsInEvent_GameCompetitorEventPickId",
                table: "CompetitorsInEvent",
                column: "GameCompetitorEventPickId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitorsInEvent_GameCompetitorPicks_GameCompetitorEventPickId",
                table: "CompetitorsInEvent",
                column: "GameCompetitorEventPickId",
                principalTable: "GameCompetitorPicks",
                principalColumn: "Id");
        }
    }
}
