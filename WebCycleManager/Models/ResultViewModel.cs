using Domain.Models;

namespace WebCycleManager.Models
{
    public class ResultViewModel
    {
        public ResultViewModel()
        {
            Results = new List<ResultItemViewModel>();
        }

        public ResultViewModel(
            int stageId, 
            int eventId, 
            int configurationId, 
            string stageName, 
            bool noScore,
            string noScoreDescription,
            int configurationItems, 
            List<ResultItemViewModel> results,
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
            Results = results;
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

        public List<ResultItemViewModel> Results { get; set; } = [];
    }
}
