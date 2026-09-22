using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
#pragma warning disable CS8981
    public partial class removepicks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameCompetitorEventId",
                table: "CompetitorsInEvent",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorsInEvent_GameCompetitorEventId",
                table: "CompetitorsInEvent",
                column: "GameCompetitorEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitorsInEvent_GameCompetitorsEvent_GameCompetitorEventId",
                table: "CompetitorsInEvent",
                column: "GameCompetitorEventId",
                principalTable: "GameCompetitorsEvent",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitorsInEvent_GameCompetitorsEvent_GameCompetitorEventId",
                table: "CompetitorsInEvent");

            migrationBuilder.DropIndex(
                name: "IX_CompetitorsInEvent_GameCompetitorEventId",
                table: "CompetitorsInEvent");

            migrationBuilder.DropColumn(
                name: "GameCompetitorEventId",
                table: "CompetitorsInEvent");
        }
    }
#pragma warning restore CS8981
}
