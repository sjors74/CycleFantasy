namespace CycleManager.Domain.Dto
{
    public class CompetitorScoreDto
    {
        public int CompetitorInEventId { get; set; }
        public int TotalScore { get; set; }
        public int LaatsteScore { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TeamName { get; set; }  
    }
}
