using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RatingCategories",
                columns: table => new
                {
                    RatingCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingCategories", x => x.RatingCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "CompetitorRatings",
                columns: table => new
                {
                    CompetitorRatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompetitorId = table.Column<int>(type: "int", nullable: false),
                    RatingCategoryId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RatingDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitorRatings", x => x.CompetitorRatingId);
                    table.ForeignKey(
                        name: "FK_CompetitorRatings_Competitors_CompetitorId",
                        column: x => x.CompetitorId,
                        principalTable: "Competitors",
                        principalColumn: "CompetitorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetitorRatings_RatingCategories_RatingCategoryId",
                        column: x => x.RatingCategoryId,
                        principalTable: "RatingCategories",
                        principalColumn: "RatingCategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorRatings_CompetitorId_RatingCategoryId_RatingDate",
                table: "CompetitorRatings",
                columns: new[] { "CompetitorId", "RatingCategoryId", "RatingDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorRatings_RatingCategoryId",
                table: "CompetitorRatings",
                column: "RatingCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompetitorRatings");

            migrationBuilder.DropTable(
                name: "RatingCategories");
        }
    }
}
