using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class AddDeelnemerScores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeelnemerPickScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameCompetitorEventPickId = table.Column<int>(type: "int", nullable: false),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeelnemerPickScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeelnemerPickScores_GameCompetitorEventPicks_GameCompetitorEventPickId",
                        column: x => x.GameCompetitorEventPickId,
                        principalTable: "GameCompetitorEventPicks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeelnemerPickScores_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "Stages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeelnemerScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameCompetitorEventId = table.Column<int>(type: "int", nullable: false),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    TotalScore = table.Column<int>(type: "int", nullable: false),
                    LaatsteScore = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeelnemerScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeelnemerScores_GameCompetitorsEvent_GameCompetitorEventId",
                        column: x => x.GameCompetitorEventId,
                        principalTable: "GameCompetitorsEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeelnemerScores_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "Stages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerPickScores_GameCompetitorEventPickId",
                table: "DeelnemerPickScores",
                column: "GameCompetitorEventPickId");

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerPickScores_StageId",
                table: "DeelnemerPickScores",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerScores_GameCompetitorEventId",
                table: "DeelnemerScores",
                column: "GameCompetitorEventId");

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerScores_StageId",
                table: "DeelnemerScores",
                column: "StageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeelnemerPickScores");

            migrationBuilder.DropTable(
                name: "DeelnemerScores");
        }
    }
}
