using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class gamecompetitorsnewestCompetitorInevent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitorsInEvent_GameCompetitorPicks_GameCompetitorEventPickId",
                table: "CompetitorsInEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_GameCompetitorPicks_GameCompetitorsEvent_GameCompetitorEventId",
                table: "GameCompetitorPicks");

            migrationBuilder.DropIndex(
                name: "IX_CompetitorsInEvent_GameCompetitorEventPickId",
                table: "CompetitorsInEvent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameCompetitorPicks",
                table: "GameCompetitorPicks");

            migrationBuilder.DropIndex(
                name: "IX_GameCompetitorPicks_GameCompetitorEventId",
                table: "GameCompetitorPicks");

            migrationBuilder.DropColumn(
                name: "GameCompetitorEventPickId",
                table: "CompetitorsInEvent");

            migrationBuilder.DropColumn(
                name: "GameCompetitorEventId",
                table: "GameCompetitorPicks");

            migrationBuilder.RenameTable(
                name: "GameCompetitorPicks",
                newName: "GameCompetitorEventPicks");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameCompetitorEventPicks",
                table: "GameCompetitorEventPicks",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CompetitorsInEventGameCompetitorEventPick",
                columns: table => new
                {
                    CompetitorsInEventCompetitorInEventId = table.Column<int>(type: "int", nullable: false),
                    GameCompetitorEventPicksId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitorsInEventGameCompetitorEventPick", x => new { x.CompetitorsInEventCompetitorInEventId, x.GameCompetitorEventPicksId });
                    table.ForeignKey(
                        name: "FK_CompetitorsInEventGameCompetitorEventPick_CompetitorsInEvent_CompetitorsInEventCompetitorInEventId",
                        column: x => x.CompetitorsInEventCompetitorInEventId,
                        principalTable: "CompetitorsInEvent",
                        principalColumn: "CompetitorInEventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetitorsInEventGameCompetitorEventPick_GameCompetitorEventPicks_GameCompetitorEventPicksId",
                        column: x => x.GameCompetitorEventPicksId,
                        principalTable: "GameCompetitorEventPicks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorsInEventGameCompetitorEventPick_GameCompetitorEventPicksId",
                table: "CompetitorsInEventGameCompetitorEventPick",
                column: "GameCompetitorEventPicksId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompetitorsInEventGameCompetitorEventPick");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameCompetitorEventPicks",
                table: "GameCompetitorEventPicks");

            migrationBuilder.RenameTable(
                name: "GameCompetitorEventPicks",
                newName: "GameCompetitorPicks");

            migrationBuilder.AddColumn<int>(
                name: "GameCompetitorEventPickId",
                table: "CompetitorsInEvent",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GameCompetitorEventId",
                table: "GameCompetitorPicks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameCompetitorPicks",
                table: "GameCompetitorPicks",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorsInEvent_GameCompetitorEventPickId",
                table: "CompetitorsInEvent",
                column: "GameCompetitorEventPickId");

            migrationBuilder.CreateIndex(
                name: "IX_GameCompetitorPicks_GameCompetitorEventId",
                table: "GameCompetitorPicks",
                column: "GameCompetitorEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitorsInEvent_GameCompetitorPicks_GameCompetitorEventPickId",
                table: "CompetitorsInEvent",
                column: "GameCompetitorEventPickId",
                principalTable: "GameCompetitorPicks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GameCompetitorPicks_GameCompetitorsEvent_GameCompetitorEventId",
                table: "GameCompetitorPicks",
                column: "GameCompetitorEventId",
                principalTable: "GameCompetitorsEvent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
