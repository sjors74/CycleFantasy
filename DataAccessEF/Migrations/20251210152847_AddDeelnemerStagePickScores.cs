using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class AddDeelnemerStagePickScores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeelnemerPickScores_Stages_StageId",
                table: "DeelnemerPickScores");

            migrationBuilder.DropForeignKey(
                name: "FK_DeelnemerScores_GameCompetitorsEvent_GameCompetitorEventId",
                table: "DeelnemerScores");

            migrationBuilder.DropForeignKey(
                name: "FK_DeelnemerStageScores_GameCompetitorsEvent_GameCompetitorEventId",
                table: "DeelnemerStageScores");

            migrationBuilder.DropForeignKey(
                name: "FK_DeelnemerStageScores_Stages_StageId",
                table: "DeelnemerStageScores");

            migrationBuilder.DropIndex(
                name: "IX_DeelnemerStageScores_GameCompetitorEventId_StageId",
                table: "DeelnemerStageScores");

            migrationBuilder.DropIndex(
                name: "IX_DeelnemerStageScores_StageId",
                table: "DeelnemerStageScores");

            migrationBuilder.DropIndex(
                name: "IX_DeelnemerPickScores_GameCompetitorEventPickId",
                table: "DeelnemerPickScores");

            migrationBuilder.DropIndex(
                name: "IX_DeelnemerPickScores_StageId",
                table: "DeelnemerPickScores");

            migrationBuilder.DropColumn(
                name: "StageId",
                table: "DeelnemerPickScores");

            migrationBuilder.RenameColumn(
                name: "Score",
                table: "DeelnemerPickScores",
                newName: "TotalScore");

            migrationBuilder.AlterColumn<int>(
                name: "StageId",
                table: "DeelnemerStageScores",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "DeelnemerStagePickScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameCompetitorEventPickId = table.Column<int>(type: "int", nullable: false),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeelnemerStagePickScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeelnemerStagePickScores_GameCompetitorEventPicks_GameCompetitorEventPickId",
                        column: x => x.GameCompetitorEventPickId,
                        principalTable: "GameCompetitorEventPicks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeelnemerStagePickScores_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "Stages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerStageScores_GameCompetitorEventId_StageId",
                table: "DeelnemerStageScores",
                columns: new[] { "GameCompetitorEventId", "StageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerPickScores_GameCompetitorEventPickId",
                table: "DeelnemerPickScores",
                column: "GameCompetitorEventPickId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerStagePickScores_GameCompetitorEventPickId_StageId",
                table: "DeelnemerStagePickScores",
                columns: new[] { "GameCompetitorEventPickId", "StageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerStagePickScores_StageId",
                table: "DeelnemerStagePickScores",
                column: "StageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeelnemerStagePickScores");

            migrationBuilder.DropIndex(
                name: "IX_DeelnemerStageScores_GameCompetitorEventId_StageId",
                table: "DeelnemerStageScores");

            migrationBuilder.DropIndex(
                name: "IX_DeelnemerPickScores_GameCompetitorEventPickId",
                table: "DeelnemerPickScores");

            migrationBuilder.RenameColumn(
                name: "TotalScore",
                table: "DeelnemerPickScores",
                newName: "Score");

            migrationBuilder.AlterColumn<int>(
                name: "StageId",
                table: "DeelnemerStageScores",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "StageId",
                table: "DeelnemerPickScores",
                type: "int",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerPickScores_GameCompetitorEventPickId",
                table: "DeelnemerPickScores",
                column: "GameCompetitorEventPickId");

            migrationBuilder.CreateIndex(
                name: "IX_DeelnemerPickScores_StageId",
                table: "DeelnemerPickScores",
                column: "StageId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeelnemerPickScores_Stages_StageId",
                table: "DeelnemerPickScores",
                column: "StageId",
                principalTable: "Stages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeelnemerScores_GameCompetitorsEvent_GameCompetitorEventId",
                table: "DeelnemerScores",
                column: "GameCompetitorEventId",
                principalTable: "GameCompetitorsEvent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeelnemerStageScores_GameCompetitorsEvent_GameCompetitorEventId",
                table: "DeelnemerStageScores",
                column: "GameCompetitorEventId",
                principalTable: "GameCompetitorsEvent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeelnemerStageScores_Stages_StageId",
                table: "DeelnemerStageScores",
                column: "StageId",
                principalTable: "Stages",
                principalColumn: "Id");
        }
    }
}
