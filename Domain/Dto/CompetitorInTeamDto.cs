namespace CycleManager.Domain.Dto
{
    public class CompetitorInTeamDto
    {
        public int CompetitorInTeamId { get; set; }
        public string CompetitorName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public int Year { get; set; }
        public bool IsNationalChampion { get; set; }
    }
}
