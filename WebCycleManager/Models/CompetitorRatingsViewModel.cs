namespace WebCycleManager.Models
{
    public class CompetitorRatingsViewModel
    {
        public List<CompetitorRatingViewModel> Ratings { get; set; } = new();
        public int MaxRating { get; set; }
    }
}
