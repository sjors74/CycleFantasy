using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class DeelnemerScore
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public int GameCompetitorEventId { get; set; }

        public int TotalScore { get; set; }

        public int LaatsteStageScore { get; set; }

        public int LaatsteStageId { get; set; } 

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    }
}
