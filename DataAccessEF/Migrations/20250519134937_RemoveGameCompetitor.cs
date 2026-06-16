using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGameCompetitor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameCompetitorsEvent_GameCompetitors_GameCompetitorId",
                table: "GameCompetitorsEvent");

            migrationBuilder.DropTable(
                name: "GameCompetitors");

            migrationBuilder.DropIndex(
                name: "IX_GameCompetitorsEvent_GameCompetitorId",
                table: "GameCompetitorsEvent");

            migrationBuilder.DropColumn(
                name: "GameCompetitorId",
                table: "GameCompetitorsEvent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameCompetitorId",
                table: "GameCompetitorsEvent",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GameCompetitors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameCompetitors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameCompetitorsEvent_GameCompetitorId",
                table: "GameCompetitorsEvent",
                column: "GameCompetitorId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameCompetitorsEvent_GameCompetitors_GameCompetitorId",
                table: "GameCompetitorsEvent",
                column: "GameCompetitorId",
                principalTable: "GameCompetitors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
