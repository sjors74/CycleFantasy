namespace WebCycleManager.Models
{
    public class CompetitorRatingViewModel
    {
        public int RatingCategoryId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Color { get; set; } = "secondary";
        public int DisplayOrder { get; set; } = 0;
        public decimal Rating { get; set; } = 0;
    }
}
