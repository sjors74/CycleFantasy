using Domain.Models;

namespace CycleManager.Domain.Models
{
    public class ScrapedStageResult
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int StageId { get; set; }
        public int BibNumber { get; set; }
        public string RiderName { get; set; } = "";
        public string TeamName { get; set; } = "";
        public int Position { get; set; }
        public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

        public int? MatchedCompetitorInEventId { get; set; }
        public CompetitorsInEvent? MatchedCompetitorInEvent { get; set; }
    }
}
