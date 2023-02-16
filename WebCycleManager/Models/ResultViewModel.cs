namespace WebCycleManager.Models
{
    public class ResultViewModel
    {
        public int StageId { get; set; }
        public int EventId { get; set; }
        public int ConfigurationId { get; set; }
        public string StageName { get; set; } = string.Empty;

        public int ConfigurationItems { get; set; }

        public List<ResultItemViewModel> Results { get; set; }

        public ResultViewModel(int stageId, int eventId, int configurationId, string stageName, int configurationItems, List<ResultItemViewModel> results)
        {
            StageId = stageId;
            EventId = eventId;
            ConfigurationId = configurationId;
            StageName = stageName;
            ConfigurationItems = configurationItems;
            Results = results;
        }
    }
}
