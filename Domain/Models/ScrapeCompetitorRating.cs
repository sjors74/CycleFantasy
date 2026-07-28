namespace CycleManager.Domain.Models
{
    public class ScrapeCompetitorRating
    {
        public int Id { get; set; }
        public string Source { get; set; } = "CyclingFlash";
        public int RatingCategoryId { get; set; }
        public string RatingCategoryCode { get; set; } = string.Empty;
        public string CompetitorName { get; set; } = string.Empty;
        public string? ProfileUrl { get; set; }
        public decimal Rating { get; set; }
        public DateTime RatingDate { get; set; }
        public bool Processed { get; set; }
        public int? CompetitorId { get; set; }
        public Guid BatchId { get; set; }
        public DateTime ImportedAt { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
