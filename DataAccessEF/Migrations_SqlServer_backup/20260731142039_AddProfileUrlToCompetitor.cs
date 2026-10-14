using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileUrlToCompetitor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ScraperName",
                table: "Competitors",
                newName: "PcsScraperName");

            migrationBuilder.AddColumn<DateTime>(
                name: "CyclingFlashLastScraped",
                table: "Competitors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CyclingFlashScraperName",
                table: "Competitors",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CyclingFlashLastScraped",
                table: "Competitors");

            migrationBuilder.DropColumn(
                name: "CyclingFlashScraperName",
                table: "Competitors");

            migrationBuilder.RenameColumn(
                name: "PcsScraperName",
                table: "Competitors",
                newName: "ScraperName");
        }
    }
}
