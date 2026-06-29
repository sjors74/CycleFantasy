using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class addResultTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpecialResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    CompetitorInEventId = table.Column<int>(type: "int", nullable: false),
                    SpecialId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecialResults_CompetitorsInEvent_CompetitorInEventId",
                        column: x => x.CompetitorInEventId,
                        principalTable: "CompetitorsInEvent",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SpecialResults_ConfigurationItemSpecials_SpecialId",
                        column: x => x.SpecialId,
                        principalTable: "ConfigurationItemSpecials",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SpecialResults_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpecialResults_CompetitorInEventId",
                table: "SpecialResults",
                column: "CompetitorInEventId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialResults_SpecialId",
                table: "SpecialResults",
                column: "SpecialId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialResults_StageId",
                table: "SpecialResults",
                column: "StageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpecialResults");
        }
    }
}
