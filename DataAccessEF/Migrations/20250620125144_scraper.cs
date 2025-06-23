using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class scraper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScrapedStageResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    BibNumber = table.Column<int>(type: "int", nullable: false),
                    RiderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeamName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MatchedCompetitorInEventId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapedStageResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScrapedStageResults_CompetitorsInEvent_MatchedCompetitorInEventId",
                        column: x => x.MatchedCompetitorInEventId,
                        principalTable: "CompetitorsInEvent",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScrapedStageResults_MatchedCompetitorInEventId",
                table: "ScrapedStageResults",
                column: "MatchedCompetitorInEventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScrapedStageResults");
        }
    }
}
