using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class AddScrapeStatusToStage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastScrapeAttempt",
                table: "Stages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSuccessfulScrape",
                table: "Stages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScrapeStatus",
                table: "Stages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastScrapeAttempt",
                table: "Stages");

            migrationBuilder.DropColumn(
                name: "LastSuccessfulScrape",
                table: "Stages");

            migrationBuilder.DropColumn(
                name: "ScrapeStatus",
                table: "Stages");
        }
    }
}
