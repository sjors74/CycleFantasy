using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class AddDeelnemerStageScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeelnemerScores_Stages_StageId",
                table: "DeelnemerScores");

            migrationBuilder.DropIndex(
                name: "IX_DeelnemerScores_GameCompetitorEventId",
                table: "DeelnemerScores");

            migrationBuilder.DropIndex(
                name: "IX_DeelnemerScores_StageId",
                table: "DeelnemerScores");

            migrationBuilder.DropColumn(
                name: "StageId",
                table: "DeelnemerScores");

            migrationBuilder.RenameColumn(
                name: "LaatsteScore",
                table: "DeelnemerScores",
                newName: "LaatsteStageScore");

            migrationBuilder.AddColumn<int>(
                name: "LaatsteStageId",
                table: "DeelnemerScores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DeelnemerStageScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameCompetitorEventId = table.Column<int>(type: "int", nullable: false),
                    StageId = table.Column<int>(type: "int", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeelnemerStageScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeelnemerStageScores_GameCompetitorsEvent_GameCompetitorEventId",
                        column: x => x.GameCompetitorEventId,
                        principalTable: "GameCompetitorsEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeelnemerStageScores_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerScores_GameCompetitorEventId",
                table: "DeelnemerScores",
                column: "GameCompetitorEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerStageScores_GameCompetitorEventId_StageId",
                table: "DeelnemerStageScores",
                columns: new[] { "GameCompetitorEventId", "StageId" },
                unique: true,
                filter: "[StageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerStageScores_StageId",
                table: "DeelnemerStageScores",
                column: "StageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeelnemerStageScores");

            migrationBuilder.DropIndex(
                name: "IX_DeelnemerScores_GameCompetitorEventId",
                table: "DeelnemerScores");

            migrationBuilder.DropColumn(
                name: "LaatsteStageId",
                table: "DeelnemerScores");

            migrationBuilder.RenameColumn(
                name: "LaatsteStageScore",
                table: "DeelnemerScores",
                newName: "LaatsteScore");

            migrationBuilder.AddColumn<int>(
                name: "StageId",
                table: "DeelnemerScores",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerScores_GameCompetitorEventId",
                table: "DeelnemerScores",
                column: "GameCompetitorEventId");

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerScores_StageId",
                table: "DeelnemerScores",
                column: "StageId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeelnemerScores_Stages_StageId",
                table: "DeelnemerScores",
                column: "StageId",
                principalTable: "Stages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
