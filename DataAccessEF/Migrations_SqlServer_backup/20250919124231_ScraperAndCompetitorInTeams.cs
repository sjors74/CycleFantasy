using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class ScraperAndCompetitorInTeams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Competitors_Teams_TeamId",
                table: "Competitors");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Country_CountryId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_CountryId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Competitors_TeamId",
                table: "Competitors");

            migrationBuilder.DropColumn(
                name: "IsNationalChampion",
                table: "Competitors");

            migrationBuilder.CreateTable(
                name: "CompetitorInTeams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompetitorId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    IsNationalChampion = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitorInTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitorInTeams_Competitors_CompetitorId",
                        column: x => x.CompetitorId,
                        principalTable: "Competitors",
                        principalColumn: "CompetitorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetitorInTeams_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScrapedCompetitors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RiderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapedCompetitors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorInTeams_CompetitorId",
                table: "CompetitorInTeams",
                column: "CompetitorId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorInTeams_TeamId",
                table: "CompetitorInTeams",
                column: "TeamId");

            migrationBuilder.Sql(@"
                INSERT INTO CompetitorInTeams (CompetitorId, TeamId, Year, IsNationalChampion)
                 SELECT CompetitorId, TeamId, 2025, 0
                 FROM Competitors
                 WHERE TeamId IS NOT NULL
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompetitorInTeams");

            migrationBuilder.DropTable(
                name: "ScrapedCompetitors");

            migrationBuilder.AddColumn<bool>(
                name: "IsNationalChampion",
                table: "Competitors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_CountryId",
                table: "Teams",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Competitors_TeamId",
                table: "Competitors",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Competitors_Teams_TeamId",
                table: "Competitors",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Country_CountryId",
                table: "Teams",
                column: "CountryId",
                principalTable: "Country",
                principalColumn: "CountryId");
        }
    }
}
