using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class HasPendingModelchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameCompetitorsEvent_EventId_UserId_TeamName",
                table: "GameCompetitorsEvent");

            migrationBuilder.AlterColumn<string>(
                name: "PcsName",
                table: "Teams",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SpecialId",
                table: "SpecialResults",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "GameCompetitorsEvent",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameCompetitorsEvent_EventId_UserId_TeamName",
                table: "GameCompetitorsEvent",
                columns: new[] { "EventId", "UserId", "TeamName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameCompetitorsEvent_EventId_UserId_TeamName",
                table: "GameCompetitorsEvent");

            migrationBuilder.AlterColumn<string>(
                name: "PcsName",
                table: "Teams",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "SpecialId",
                table: "SpecialResults",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "GameCompetitorsEvent",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_GameCompetitorsEvent_EventId_UserId_TeamName",
                table: "GameCompetitorsEvent",
                columns: new[] { "EventId", "UserId", "TeamName" },
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }
    }
}
