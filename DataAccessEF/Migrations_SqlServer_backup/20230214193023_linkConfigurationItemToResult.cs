using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class linkConfigurationItemToResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Results_ConfigurationItems_ConfigurationItemId",
                table: "Results");

            migrationBuilder.AlterColumn<int>(
                name: "ConfigurationItemId",
                table: "Results",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Results_ConfigurationItems_ConfigurationItemId",
                table: "Results",
                column: "ConfigurationItemId",
                principalTable: "ConfigurationItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Results_ConfigurationItems_ConfigurationItemId",
                table: "Results");

            migrationBuilder.AlterColumn<int>(
                name: "ConfigurationItemId",
                table: "Results",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Results_ConfigurationItems_ConfigurationItemId",
                table: "Results",
                column: "ConfigurationItemId",
                principalTable: "ConfigurationItems",
                principalColumn: "Id");
        }
    }
}
