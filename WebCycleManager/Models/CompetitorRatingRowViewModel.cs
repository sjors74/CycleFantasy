namespace WebCycleManager.Models
{
    public class CompetitorRatingRowViewModel
    {
        public int CompetitorId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime RatingDate { get; set; }
    }
}
