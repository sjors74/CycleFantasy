namespace CycleManager.Domain.Dto
{
    public class CompetitorScoreDto
    {
        public int CompetitorInEventId { get; set; }
        public int NormalScore { get; set; }
        public int SpecialScore { get; set; }
        public int TotalScore => NormalScore + SpecialScore;
        public int LaatsteScore { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TeamName { get; set; }  
    }
}
