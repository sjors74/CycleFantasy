namespace WebCycleManager.Models
{
    public class GameCompetitorRatingViewModel
    {
        public int RatingCategoryId { get; set; }
        public string RatingCategoryName { get; set; } = string.Empty;
        public decimal Rating { get; set; } = 0;
        public string Color { get; set; } = string.Empty;
    }
}
