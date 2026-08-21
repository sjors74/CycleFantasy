using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class UniqueConstraintForDeelnemerPool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameCompetitorsEvent_EventId",
                table: "GameCompetitorsEvent");

            migrationBuilder.AlterColumn<string>(
                name: "TeamName",
                table: "GameCompetitorsEvent",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_GameCompetitorsEvent_EventId_UserId_TeamName",
                table: "GameCompetitorsEvent",
                columns: new[] { "EventId", "UserId", "TeamName" },
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameCompetitorsEvent_EventId_UserId_TeamName",
                table: "GameCompetitorsEvent");

            migrationBuilder.AlterColumn<string>(
                name: "TeamName",
                table: "GameCompetitorsEvent",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_GameCompetitorsEvent_EventId",
                table: "GameCompetitorsEvent",
                column: "EventId");
        }
    }
}
