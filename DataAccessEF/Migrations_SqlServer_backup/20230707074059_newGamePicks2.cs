using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class newGamePicks2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompetitorsInEventId",
                table: "GameCompetitorEventPicks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GameCompetitorEventPicks_CompetitorsInEventId",
                table: "GameCompetitorEventPicks",
                column: "CompetitorsInEventId");

            migrationBuilder.CreateIndex(
                name: "IX_GameCompetitorEventPicks_GameCompetitorEventId",
                table: "GameCompetitorEventPicks",
                column: "GameCompetitorEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameCompetitorEventPicks_CompetitorsInEvent_CompetitorsInEventId",
                table: "GameCompetitorEventPicks",
                column: "CompetitorsInEventId",
                principalTable: "CompetitorsInEvent",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_GameCompetitorEventPicks_GameCompetitorsEvent_GameCompetitorEventId",
                table: "GameCompetitorEventPicks",
                column: "GameCompetitorEventId",
                principalTable: "GameCompetitorsEvent",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameCompetitorEventPicks_CompetitorsInEvent_CompetitorsInEventId",
                table: "GameCompetitorEventPicks");

            migrationBuilder.DropForeignKey(
                name: "FK_GameCompetitorEventPicks_GameCompetitorsEvent_GameCompetitorEventId",
                table: "GameCompetitorEventPicks");

            migrationBuilder.DropIndex(
                name: "IX_GameCompetitorEventPicks_CompetitorsInEventId",
                table: "GameCompetitorEventPicks");

            migrationBuilder.DropIndex(
                name: "IX_GameCompetitorEventPicks_GameCompetitorEventId",
                table: "GameCompetitorEventPicks");

            migrationBuilder.DropColumn(
                name: "CompetitorsInEventId",
                table: "GameCompetitorEventPicks");
        }
    }
}
