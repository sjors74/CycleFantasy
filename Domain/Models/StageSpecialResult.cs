using CycleManager.Domain.Enums;
using Domain.Models;

namespace CycleManager.Domain.Models
{
    public class StageSpecialResult
    {
        public Guid Id { get; set; }

        public int StageId { get; set; }

        public QuestionType QuestionType { get; set; }

        public int CompetitorInEventId { get; set; }

        public DateTime ImportedAt { get; set; }

        public virtual Stage Stage { get; set; } = null!;

        public virtual CompetitorsInEvent CompetitorInEvent { get; set; } = null!;
    }
}
