using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class renameScraperTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StageSpecialResults");

            migrationBuilder.CreateTable(
                name: "ScrapedSpecialResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    QuestionType = table.Column<int>(type: "int", nullable: false),
                    BibNumber = table.Column<int>(type: "int", nullable: false),
                    CompetitorInEventId = table.Column<int>(type: "int", nullable: true),
                    ImportedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapedSpecialResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScrapedSpecialResults_CompetitorsInEvent_CompetitorInEventId",
                        column: x => x.CompetitorInEventId,
                        principalTable: "CompetitorsInEvent",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScrapedSpecialResults_CompetitorInEventId",
                table: "ScrapedSpecialResults",
                column: "CompetitorInEventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScrapedSpecialResults");

            migrationBuilder.CreateTable(
                name: "StageSpecialResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompetitorInEventId = table.Column<int>(type: "int", nullable: true),
                    BibNumber = table.Column<int>(type: "int", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuestionType = table.Column<int>(type: "int", nullable: false),
                    StageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageSpecialResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StageSpecialResults_CompetitorsInEvent_CompetitorInEventId",
                        column: x => x.CompetitorInEventId,
                        principalTable: "CompetitorsInEvent",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_StageSpecialResults_CompetitorInEventId",
                table: "StageSpecialResults",
                column: "CompetitorInEventId");
        }
    }
}
