using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdateScrapeCompetitorRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScrapeCompetitorRatings_RatingCategories_RatingCategoryId",
                table: "ScrapeCompetitorRatings");

            migrationBuilder.DropIndex(
                name: "IX_ScrapeCompetitorRatings_RatingCategoryId",
                table: "ScrapeCompetitorRatings");

            migrationBuilder.AddColumn<string>(
                name: "RatingCategoryCode",
                table: "ScrapeCompetitorRatings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatingCategoryCode",
                table: "ScrapeCompetitorRatings");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapeCompetitorRatings_RatingCategoryId",
                table: "ScrapeCompetitorRatings",
                column: "RatingCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScrapeCompetitorRatings_RatingCategories_RatingCategoryId",
                table: "ScrapeCompetitorRatings",
                column: "RatingCategoryId",
                principalTable: "RatingCategories",
                principalColumn: "RatingCategoryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
