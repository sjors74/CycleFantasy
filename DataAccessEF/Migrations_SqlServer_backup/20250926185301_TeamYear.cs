using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class TeamYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeamYearId",
                table: "CompetitorInTeams",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TeamYear",
                columns: table => new
                {
                    TeamYearId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamYear", x => x.TeamYearId);
                    table.ForeignKey(
                        name: "FK_TeamYear_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorInTeams_TeamYearId",
                table: "CompetitorInTeams",
                column: "TeamYearId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamYear_TeamId",
                table: "TeamYear",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitorInTeams_TeamYear_TeamYearId",
                table: "CompetitorInTeams",
                column: "TeamYearId",
                principalTable: "TeamYear",
                principalColumn: "TeamYearId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitorInTeams_TeamYear_TeamYearId",
                table: "CompetitorInTeams");

            migrationBuilder.DropTable(
                name: "TeamYear");

            migrationBuilder.DropIndex(
                name: "IX_CompetitorInTeams_TeamYearId",
                table: "CompetitorInTeams");

            migrationBuilder.DropColumn(
                name: "TeamYearId",
                table: "CompetitorInTeams");
        }
    }
}
