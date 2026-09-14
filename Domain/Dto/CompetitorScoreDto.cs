namespace CycleManager.Domain.Dto
{
    public class CompetitorScoreDto
    {
        public int CompetitorInEventId { get; set; }
        public int NormalScore { get; set; }
        public int SpecialScore { get; set; }
        public int TotalScore => NormalScore + SpecialScore;
        public int LaatsteScore { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
    }
}
