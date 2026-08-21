using CycleManager.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class DeelnemerStagePickSpecialScore
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public int GameCompetitorEventPickId { get; set; }

        [Required]
        public int StageId { get; set; }

        public QuestionType QuestionType { get; set; }

        [Required]
        public int Score { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public virtual GameCompetitorEventPick Pick { get; set; } = null!;
    }
}
