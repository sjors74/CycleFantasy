using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class LinkCompetitorInEventToCompetitorInTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitorsInEvent_Competitors_CompetitorId",
                table: "CompetitorsInEvent");

            migrationBuilder.DropIndex(
                name: "IX_CompetitorsInEvent_CompetitorId",
                table: "CompetitorsInEvent");

            migrationBuilder.AddColumn<int>(
                name: "CompetitorInTeamId",
                table: "CompetitorsInEvent",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
        UPDATE cie
        SET cie.CompetitorInTeamId = cit.Id
        FROM CompetitorsInEvent cie
        INNER JOIN Competitors c ON c.CompetitorId = cie.CompetitorId
        INNER JOIN CompetitorInTeams cit ON cit.CompetitorId = c.CompetitorId AND cit.TeamId = c.TeamId
    ");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorsInEvent_CompetitorInTeamId",
                table: "CompetitorsInEvent",
                column: "CompetitorInTeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitorsInEvent_CompetitorInTeams_CompetitorInTeamId",
                table: "CompetitorsInEvent",
                column: "CompetitorInTeamId",
                principalTable: "CompetitorInTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitorsInEvent_CompetitorInTeams_CompetitorInTeamId",
                table: "CompetitorsInEvent");

            migrationBuilder.DropIndex(
                name: "IX_CompetitorsInEvent_CompetitorInTeamId",
                table: "CompetitorsInEvent");

            migrationBuilder.DropColumn(
                name: "CompetitorInTeamId",
                table: "CompetitorsInEvent");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorsInEvent_CompetitorId",
                table: "CompetitorsInEvent",
                column: "CompetitorId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitorsInEvent_Competitors_CompetitorId",
                table: "CompetitorsInEvent",
                column: "CompetitorId",
                principalTable: "Competitors",
                principalColumn: "CompetitorId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
