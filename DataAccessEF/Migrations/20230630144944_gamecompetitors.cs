using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class gamecompetitors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameCompetitorEventPickId",
                table: "CompetitorsInEvent",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GameCompetitors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeamName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameCompetitors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameCompetitorsEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameCompetitorId = table.Column<int>(type: "int", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameCompetitorsEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameCompetitorsEvent_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameCompetitorsEvent_GameCompetitors_GameCompetitorId",
                        column: x => x.GameCompetitorId,
                        principalTable: "GameCompetitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameCompetitorPicks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameCompetitorEventId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameCompetitorPicks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameCompetitorPicks_GameCompetitorsEvent_GameCompetitorEventId",
                        column: x => x.GameCompetitorEventId,
                        principalTable: "GameCompetitorsEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorsInEvent_GameCompetitorEventPickId",
                table: "CompetitorsInEvent",
                column: "GameCompetitorEventPickId");

            migrationBuilder.CreateIndex(
                name: "IX_GameCompetitorPicks_GameCompetitorEventId",
                table: "GameCompetitorPicks",
                column: "GameCompetitorEventId");

            migrationBuilder.CreateIndex(
                name: "IX_GameCompetitorsEvent_EventId",
                table: "GameCompetitorsEvent",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_GameCompetitorsEvent_GameCompetitorId",
                table: "GameCompetitorsEvent",
                column: "GameCompetitorId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitorsInEvent_GameCompetitorPicks_GameCompetitorEventPickId",
                table: "CompetitorsInEvent",
                column: "GameCompetitorEventPickId",
                principalTable: "GameCompetitorPicks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitorsInEvent_GameCompetitorPicks_GameCompetitorEventPickId",
                table: "CompetitorsInEvent");

            migrationBuilder.DropTable(
                name: "GameCompetitorPicks");

            migrationBuilder.DropTable(
                name: "GameCompetitorsEvent");

            migrationBuilder.DropTable(
                name: "GameCompetitors");

            migrationBuilder.DropIndex(
                name: "IX_CompetitorsInEvent_GameCompetitorEventPickId",
                table: "CompetitorsInEvent");

            migrationBuilder.DropColumn(
                name: "GameCompetitorEventPickId",
                table: "CompetitorsInEvent");
        }
    }
}
