namespace CycleManager.Domain.Dto
{
    public class CompetitorRatingDto
    {
        public int RatingCategoryId { get; set; }
        public int Rating { get; set; }
        public string Code { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
}
