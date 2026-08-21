using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class TestFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitorInTeams_TeamYear_TeamYearId",
                table: "CompetitorInTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitorInTeams_Teams_TeamId",
                table: "CompetitorInTeams");

            migrationBuilder.DropIndex(
                name: "IX_CompetitorInTeams_TeamId",
                table: "CompetitorInTeams");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "CompetitorInTeams");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "CompetitorInTeams");

            migrationBuilder.AlterColumn<int>(
                name: "TeamYearId",
                table: "CompetitorInTeams",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitorInTeams_TeamYear_TeamYearId",
                table: "CompetitorInTeams",
                column: "TeamYearId",
                principalTable: "TeamYear",
                principalColumn: "TeamYearId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitorInTeams_TeamYear_TeamYearId",
                table: "CompetitorInTeams");

            migrationBuilder.AlterColumn<int>(
                name: "TeamYearId",
                table: "CompetitorInTeams",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "CompetitorInTeams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "CompetitorInTeams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorInTeams_TeamId",
                table: "CompetitorInTeams",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitorInTeams_TeamYear_TeamYearId",
                table: "CompetitorInTeams",
                column: "TeamYearId",
                principalTable: "TeamYear",
                principalColumn: "TeamYearId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitorInTeams_Teams_TeamId",
                table: "CompetitorInTeams",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
