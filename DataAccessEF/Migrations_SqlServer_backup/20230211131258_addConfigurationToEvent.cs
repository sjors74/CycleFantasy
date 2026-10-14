using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class addConfigurationToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConfigurationId",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_ConfigurationId",
                table: "Events",
                column: "ConfigurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Configurations_ConfigurationId",
                table: "Events",
                column: "ConfigurationId",
                principalTable: "Configurations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Configurations_ConfigurationId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_ConfigurationId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ConfigurationId",
                table: "Events");
        }
    }
}
