namespace WebCycleManager.Models
{
    public class TeamDetailsViewModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        public int SelectedYear { get; set; }
        public List<int> AvailableYears { get; set; } = new();
        public List<CompetitorViewModel> Competitors { get; set; } = new();
    }
}
