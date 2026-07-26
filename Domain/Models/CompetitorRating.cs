using Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace CycleManager.Domain.Models
{
    public class CompetitorRating
    {
        [Key]
        public int CompetitorRatingId { get; set; }
        public int CompetitorId { get; set; }
        public required Competitor Competitor { get; set; }
        public int RatingCategoryId { get; set; }
        public required RatingCategory RatingCategory { get; set; }
        public decimal Rating { get; set; }
        public DateTime RatingDate { get; set; }
    }
}
