using CycleManager.Domain.Enums;
using Domain.Models;

namespace CycleManager.Domain.Models
{
    public class ScrapedSpecialResult
    {
        public Guid Id { get; set; }

        public int StageId { get; set; }

        public QuestionType QuestionType { get; set; }

        public int BibNumber { get; set; }

        public int? CompetitorInEventId { get; set; }

        public DateTime ImportedAt { get; set; }

        public virtual CompetitorsInEvent? CompetitorInEvent { get; set; }
    }
}
