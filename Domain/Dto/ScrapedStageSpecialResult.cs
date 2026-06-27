using CycleManager.Domain.Enums;

namespace CycleManager.Domain.Dto
{
    public class ScrapedStageSpecialResult
    {
        public int StageId { get; set; }
        public QuestionType QuestionType { get; set; }
        public int BibNumber { get; set; }
        public string RiderName { get; set; } = "";
        public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
    }
}
