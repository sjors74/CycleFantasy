using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class newGamePicks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompetitorsInEventGameCompetitorEventPick");

            migrationBuilder.RenameColumn(
                name: "CompetitorInEventId",
                table: "CompetitorsInEvent",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "CompetitorInEventId",
                table: "GameCompetitorEventPicks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompetitorInEventId",
                table: "GameCompetitorEventPicks");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "CompetitorsInEvent",
                newName: "CompetitorInEventId");

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
    }
}
