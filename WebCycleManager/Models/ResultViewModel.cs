using Domain.Models;

namespace WebCycleManager.Models
{
    public class ResultViewModel
    {
        public ResultViewModel()
        {
            Rows = new List<StageResultRowViewModel>();
        }

        public ResultViewModel(
            int stageId, 
            int eventId, 
            int configurationId, 
            string stageName, 
            bool noScore,
            string noScoreDescription,
            int configurationItems, 
            List<StageResultRowViewModel> rows,
            List<CompetitorsInEvent> competitorsInEvent
            )
        {
            StageId = stageId;
            EventId = eventId;
            ConfigurationId = configurationId;
            StageName = stageName;
            NoScore = noScore;
            NoScoreDescription = noScoreDescription;
            ConfigurationItems = configurationItems;
            Rows = rows;
            Competitors = competitorsInEvent;
        }

        public int StageId { get; set; }
        public int EventId { get; set; }
        public int ConfigurationId { get; set; }
        public List<CompetitorsInEvent> Competitors { get; set; } = [];
        public int CompetitorInEventId { get; set; }
        public string CompetitorName { get; set; } = string.Empty;
        public string StageName { get; set; } = string.Empty;
        public bool NoScore { get; set; }
        public string? NoScoreDescription { get; set; }
        public int ConfigurationItems { get; set; }
        public List<StageResultRowViewModel> Rows { get; set; }
    }
}
