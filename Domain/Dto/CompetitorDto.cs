using CycleManager.Domain.Dto;

namespace Domain.Dto
{
    public class CompetitorDto
    {
        public int CompetitorId { get; set; }
        public int CompetitorInTeamId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PcsName { get; set; } = string.Empty;
        public string? ScraperName { get; set; }
        public string CountryShort { get; set; } = string.Empty;
        public string EventNumber { get; set; } = string.Empty;
        public int Punten { get; set; }
        public bool InSelectie { get; set; } = false;
        public string CurrentTeamName { get; set; } = string.Empty;
        public bool IsNationalChampion { get; set; } = false;
        public string CompetitorName => $"{FirstName} {LastName}";


        public List<CompetitorInTeamDto> Teams { get; set; } = new();

    }
}
