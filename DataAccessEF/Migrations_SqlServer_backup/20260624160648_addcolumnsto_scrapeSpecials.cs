using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class addcolumnsto_scrapeSpecials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StageSpecialResults_Competitors_CompetitorId",
                table: "StageSpecialResults");

            migrationBuilder.DropForeignKey(
                name: "FK_StageSpecialResults_Stages_StageId",
                table: "StageSpecialResults");

            migrationBuilder.DropIndex(
                name: "IX_StageSpecialResults_CompetitorId",
                table: "StageSpecialResults");

            migrationBuilder.DropIndex(
                name: "IX_StageSpecialResults_StageId",
                table: "StageSpecialResults");

            migrationBuilder.RenameColumn(
                name: "CompetitorId",
                table: "StageSpecialResults",
                newName: "BibNumber");

            migrationBuilder.AddColumn<int>(
                name: "CompetitorInEventId",
                table: "StageSpecialResults",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StageSpecialResults_CompetitorInEventId",
                table: "StageSpecialResults",
                column: "CompetitorInEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_StageSpecialResults_CompetitorsInEvent_CompetitorInEventId",
                table: "StageSpecialResults",
                column: "CompetitorInEventId",
                principalTable: "CompetitorsInEvent",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StageSpecialResults_CompetitorsInEvent_CompetitorInEventId",
                table: "StageSpecialResults");

            migrationBuilder.DropIndex(
                name: "IX_StageSpecialResults_CompetitorInEventId",
                table: "StageSpecialResults");

            migrationBuilder.DropColumn(
                name: "CompetitorInEventId",
                table: "StageSpecialResults");

            migrationBuilder.RenameColumn(
                name: "BibNumber",
                table: "StageSpecialResults",
                newName: "CompetitorId");

            migrationBuilder.CreateIndex(
                name: "IX_StageSpecialResults_CompetitorId",
                table: "StageSpecialResults",
                column: "CompetitorId");

            migrationBuilder.CreateIndex(
                name: "IX_StageSpecialResults_StageId",
                table: "StageSpecialResults",
                column: "StageId");

            migrationBuilder.AddForeignKey(
                name: "FK_StageSpecialResults_Competitors_CompetitorId",
                table: "StageSpecialResults",
                column: "CompetitorId",
                principalTable: "Competitors",
                principalColumn: "CompetitorId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StageSpecialResults_Stages_StageId",
                table: "StageSpecialResults",
                column: "StageId",
                principalTable: "Stages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
