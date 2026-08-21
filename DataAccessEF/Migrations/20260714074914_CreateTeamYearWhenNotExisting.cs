using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class CreateTeamYearWhenNotExisting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO TeamYear (TeamId, SeasonYearId, year, Name)
                SELECT
                    t.TeamId,
                    sy.SeasonYearId,
                    sy.Year,
                    t.CurrentTeamName
                FROM Teams t
                CROSS JOIN SeasonYears sy
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM TeamYear ty
                    WHERE ty.TeamId = t.TeamId
                      AND ty.SeasonYearId = sy.SeasonYearId
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //geen down mogelijk
        }
    }
}
