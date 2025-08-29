namespace Domain.Dto
{
    public class CompetitorDto
    {
        public int CompetitorId { get; set; }
        public string CompetitorName { get; set; } = string.Empty;
        public string PcsName { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string CountryShort { get; set; } = string.Empty;
        public string EventNumber { get; set; } = string.Empty;
        public int Punten { get; set; }
        public bool InSelectie { get; set; } = false;
        public bool IsNationalChampion { get; set; } = false;

    }
}
