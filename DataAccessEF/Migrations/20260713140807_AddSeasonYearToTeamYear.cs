using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class AddSeasonYearToTeamYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. SeasonYearId tijdelijk nullable toevoegen
            migrationBuilder.AddColumn<int>(
                name: "SeasonYearId",
                table: "TeamYear",
                type: "int",
                nullable: true);

            // 2. Ontbrekende seizoenen aanmaken
            migrationBuilder.Sql(@"
                INSERT INTO SeasonYears (Year, Active)
                SELECT DISTINCT Year, 1
                FROM TeamYear
                WHERE Year NOT IN
                (
                    SELECT Year
                    FROM SeasonYears
                )
            ");

            migrationBuilder.Sql(@"
                UPDATE TeamYear
                SET SeasonYearId =
                (
                    SELECT SeasonYearId
                    FROM SeasonYears
                    WHERE SeasonYears.Year = TeamYear.Year
                )
            ");

            migrationBuilder.CreateIndex(
                name: "IX_TeamYear_SeasonYearId",
                table: "TeamYear",
                column: "SeasonYearId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamYear_SeasonYears_SeasonYearId",
                table: "TeamYear",
                column: "SeasonYearId",
                principalTable: "SeasonYears",
                principalColumn: "SeasonYearId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamYear_SeasonYears_SeasonYearId",
                table: "TeamYear");

            migrationBuilder.DropIndex(
                name: "IX_TeamYear_SeasonYearId",
                table: "TeamYear");

            migrationBuilder.DropColumn(
                name: "SeasonYearId",
                table: "TeamYear");

        }
    }
}
