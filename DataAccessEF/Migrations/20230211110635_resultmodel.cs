using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class resultmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Stages_EventId",
                table: "Stages",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stages_Events_EventId",
                table: "Stages",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stages_Events_EventId",
                table: "Stages");

            migrationBuilder.DropIndex(
                name: "IX_Stages_EventId",
                table: "Stages");
        }
    }
}
