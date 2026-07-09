using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class fixSpecialResultsStageRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpecialResults_Stages_StageId",
                table: "SpecialResults");

            migrationBuilder.AddForeignKey(
                name: "FK_SpecialResults_Stages_StageId",
                table: "SpecialResults",
                column: "StageId",
                principalTable: "Stages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpecialResults_Stages_StageId",
                table: "SpecialResults");

            migrationBuilder.AddForeignKey(
                name: "FK_SpecialResults_Stages_StageId",
                table: "SpecialResults",
                column: "StageId",
                principalTable: "Stages",
                principalColumn: "Id");
        }
    }
}
