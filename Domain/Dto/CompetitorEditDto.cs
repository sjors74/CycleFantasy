namespace CycleManager.Domain.Dto
{
    public class CompetitorEditDto
    {
        public int CompetitorId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; }= string.Empty;
        public string? PcsName { get; set; }
        public string? ScraperName { get; set; }
        public int CountryId { get; set; }
        public int? SelectedTeamYearId { get; set; }
        public int SelectedSeasonYearId { get; set; }

        public IEnumerable<SeasonYearDto> AvailableYears { get; set; } = new List<SeasonYearDto>();
        public IEnumerable<TeamYearDto> Teams { get; set; } = new List<TeamYearDto>();
        public IEnumerable<CountryDto> Countries { get; set; } = new List<CountryDto>();

        public List<CompetitorInTeamDto> CompetitorInTeams { get; set; } = new();
    }
}
