using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class DeelnemerScore
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public int GameCompetitorEventId { get; set; }

        public int? StageId { get; set; }

        public int TotalScore { get; set; }

        public int LaatsteScore { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public virtual GameCompetitorEvent GameCompetitorEvent { get; set; }
        public virtual Stage? Stage { get; set; }
    }
}
