namespace CycleManager.Domain.Dto
{
    public class CompetitorEditDto
    {
        public int CompetitorId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; }= string.Empty;
        public string? PcsName { get; set; }
        public string? PcsScraperName { get; set; }
        public string? CyclingFlashScraperName { get; set; }
        public DateTime? CyclingFlahsLastScraped { get; set; }
        public int CountryId { get; set; }
        public int? SelectedTeamYearId { get; set; }
        public int SelectedSeasonYearId { get; set; }

        public IEnumerable<SeasonYearDto> AvailableYears { get; set; } = new List<SeasonYearDto>();
        public IEnumerable<TeamYearDto> Teams { get; set; } = new List<TeamYearDto>();
        public IEnumerable<CountryDto> Countries { get; set; } = new List<CountryDto>();

        public List<CompetitorInTeamDto> CompetitorInTeams { get; set; } = new();

        public IEnumerable<RatingCategoryDto> RatingCategories { get; set; } = new List<RatingCategoryDto>();
        public IEnumerable<CompetitorRatingDto> Ratings { get; set; } = new List<CompetitorRatingDto>();
    }
}
