namespace WebCycleManager.Models
{
    public class SpecialResultItemViewModel
    {
        public int SpecialId { get; set; }

        public string SpecialName { get; set; } = string.Empty;

        public int? SelectedCompetitorId { get; set; }

        public string CompetitorName { get; set; } = string.Empty;
    }
}
