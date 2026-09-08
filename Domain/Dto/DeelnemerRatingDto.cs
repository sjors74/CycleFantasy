namespace CycleManager.Domain.Dto
{
    public class DeelnemerRatingDto
    {
        public int GameCompetitorEventId { get; set; }
        public int RatingCategoryId { get; set; }
        public string RatingCategoryName { get; set; } = string.Empty;
        public string Color { get; set; } = "secondary";
        public int DisplayOrder { get; set; } = 0;
        public decimal Rating { get; set; } = 0;
    }
}
