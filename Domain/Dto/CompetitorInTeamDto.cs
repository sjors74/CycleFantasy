namespace CycleManager.Domain.Dto
{
    public class CompetitorInTeamDto
    {
        public int CompetitorInTeamId { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public int Year { get; set; }
        public bool IsNationalChampion { get; set; }
    }
}
