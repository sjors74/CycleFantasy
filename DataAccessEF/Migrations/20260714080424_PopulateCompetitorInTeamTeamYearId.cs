using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessEF.Migrations
{
    /// <inheritdoc />
    public partial class PopulateCompetitorInTeamTeamYearId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE cit
                SET TeamYearId = ty.TeamYearId
                FROM CompetitorInTeams cit
                INNER JOIN SeasonYears sy
                    ON sy.Year = cit.Year
                INNER JOIN TeamYear ty
                    ON ty.TeamId = cit.TeamId
                   AND ty.SeasonYearId = sy.SeasonYearId;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data migration: no automatic rollback.
        }
    }
}
