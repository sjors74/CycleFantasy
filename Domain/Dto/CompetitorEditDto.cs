namespace CycleManager.Domain.Dto
{
    public class CompetitorEditDto
    {
        public int CompetitorId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; }= string.Empty;
        public string? PcsName { get; set; } = string.Empty;
        public string? PcsScraperName { get; set; }
        public string? CyclingFlashScraperName { get; set; }
        public DateTime? CyclingFlahsLastScraped { get; set; }
        public int CountryId { get; set; }
        public int? SelectedTeamYearId { get; set; }
        public int SelectedSeasonYearId { get; set; }

        public IEnumerable<SeasonYearDto> AvailableYears { get; set; } = [];
        public IEnumerable<TeamYearDto> Teams { get; set; } = [];
        public IEnumerable<CountryDto> Countries { get; set; } = [];

        public List<CompetitorInTeamDto> CompetitorInTeams { get; set; } = [];

        public IEnumerable<RatingCategoryDto> RatingCategories { get; set; } = [];
        public IEnumerable<CompetitorRatingDto> Ratings { get; set; } = [];
    }
}
