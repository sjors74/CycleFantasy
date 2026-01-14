using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class NavigateCountryInTeams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Teams_CountryId",
                table: "Teams",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Country_CountryId",
                table: "Teams",
                column: "CountryId",
                principalTable: "Country",
                principalColumn: "CountryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Country_CountryId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_CountryId",
                table: "Teams");
        }
    }
}
