namespace WebCycleManager.Models
{
    public class PointsCompetitorInEventViewModel
    {
        public int EventId { get; set; }
        public int Ranking { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Points { get; set; }
        public int CompetitorEventId { get; set; }
        public string CompetitorName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }
    }
}
