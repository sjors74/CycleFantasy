namespace WebCycleManager.Models
{
    public class GameCompetitorRatingsViewModel
    {
        public List<GameCompetitorRatingViewModel> Ratings { get; set; } = new();
        public int MaxRating { get; set; }
    }
}
