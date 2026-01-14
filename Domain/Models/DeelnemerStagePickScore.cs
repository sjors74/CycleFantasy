using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class DeelnemerStagePickScore
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public int GameCompetitorEventPickId { get; set; }

        [Required]
        public int StageId { get; set; }

        [Required]
        public int Score { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public virtual GameCompetitorEventPick Pick { get; set; }
        public virtual Stage Stage { get; set; }
    }
}
