using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class addspecialpickscore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeelnemerStagePickSpecialScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameCompetitorEventPickId = table.Column<int>(type: "int", nullable: false),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    QuestionType = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeelnemerStagePickSpecialScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeelnemerStagePickSpecialScores_GameCompetitorEventPicks_GameCompetitorEventPickId",
                        column: x => x.GameCompetitorEventPickId,
                        principalTable: "GameCompetitorEventPicks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerStagePickSpecialScores_GameCompetitorEventPickId",
                table: "DeelnemerStagePickSpecialScores",
                column: "GameCompetitorEventPickId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeelnemerStagePickSpecialScores");
        }
    }
}
