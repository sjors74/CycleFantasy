namespace WebCycleManager.Models
{
    public class ResultItemViewModel
    {
        public int Id { get; set; }
        public int Position { get; set; }
        public int SelectedCompetitorId { get; set; }
        public string CompetitorName { get; set; } = string.Empty;
    }
}
