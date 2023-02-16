using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class competitorInevent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Results_Competitors_CompetitorId",
                table: "Results");

            migrationBuilder.RenameColumn(
                name: "CompetitorId",
                table: "Results",
                newName: "CompetitorInEventId");

            migrationBuilder.RenameIndex(
                name: "IX_Results_CompetitorId",
                table: "Results",
                newName: "IX_Results_CompetitorInEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Results_CompetitorsInEvent_CompetitorInEventId",
                table: "Results",
                column: "CompetitorInEventId",
                principalTable: "CompetitorsInEvent",
                principalColumn: "CompetitorInEventId",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Results_CompetitorsInEvent_CompetitorInEventId",
                table: "Results");

            migrationBuilder.RenameColumn(
                name: "CompetitorInEventId",
                table: "Results",
                newName: "CompetitorId");

            migrationBuilder.RenameIndex(
                name: "IX_Results_CompetitorInEventId",
                table: "Results",
                newName: "IX_Results_CompetitorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Competitors_CompetitorId",
                table: "Results",
                column: "CompetitorId",
                principalTable: "Competitors",
                principalColumn: "CompetitorId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
