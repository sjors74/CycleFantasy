namespace CycleManager.Domain.Dto
{
    public class CompetitorEditDto
    {
        public int CompetitorId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; }= string.Empty;
        public string? PcsName { get; set; }
        public int CountryId { get; set; }
        public int SelectedTeamId { get; set; }
        public int SelectedYear { get; set; }
        public bool IsNationalChampion { get; set; }

        public IEnumerable<int> AvailableYears { get; set; }
        public IEnumerable<TeamDto> Teams { get; set; }
        public IEnumerable<CountryDto> Countries { get; set; }
    }
}
