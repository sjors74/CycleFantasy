using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class AddScrapingCompetitorRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScrapeCompetitorRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RatingCategoryId = table.Column<int>(type: "int", nullable: false),
                    CompetitorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RatingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Processed = table.Column<bool>(type: "bit", nullable: false),
                    CompetitorId = table.Column<int>(type: "int", nullable: true),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapeCompetitorRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScrapeCompetitorRatings_RatingCategories_RatingCategoryId",
                        column: x => x.RatingCategoryId,
                        principalTable: "RatingCategories",
                        principalColumn: "RatingCategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScrapeCompetitorRatings_RatingCategoryId",
                table: "ScrapeCompetitorRatings",
                column: "RatingCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScrapeCompetitorRatings");
        }
    }
}
