using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingScrapeProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileUrl",
                table: "ScrapeCompetitorRatings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxPages",
                table: "RatingCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RefreshOrder",
                table: "RatingCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RatingScrapeProgress",
                columns: table => new
                {
                    RatingScrapeProgressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RatingCategoryId = table.Column<int>(type: "int", nullable: false),
                    LastPage = table.Column<int>(type: "int", nullable: false),
                    LastScrapeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorCount = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingScrapeProgress", x => x.RatingScrapeProgressId);
                    table.ForeignKey(
                        name: "FK_RatingScrapeProgress_RatingCategories_RatingCategoryId",
                        column: x => x.RatingCategoryId,
                        principalTable: "RatingCategories",
                        principalColumn: "RatingCategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RatingScrapeProgress_RatingCategoryId",
                table: "RatingScrapeProgress",
                column: "RatingCategoryId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RatingScrapeProgress");

            migrationBuilder.DropColumn(
                name: "ProfileUrl",
                table: "ScrapeCompetitorRatings");

            migrationBuilder.DropColumn(
                name: "MaxPages",
                table: "RatingCategories");

            migrationBuilder.DropColumn(
                name: "RefreshOrder",
                table: "RatingCategories");
        }
    }
}
