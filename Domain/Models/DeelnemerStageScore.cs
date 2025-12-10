using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class DeelnemerStageScore
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public int GameCompetitorEventId { get; set; }

        [Required]
        public int StageId { get; set; }

        [Required]
        public int Score { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
