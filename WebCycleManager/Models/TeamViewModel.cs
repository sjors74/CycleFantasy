using System.ComponentModel;

namespace WebCycleManager.Models
{
    public class TeamViewModel
    {
        public int Id { get; set; }
        [DisplayName("Team naam")]
        public string TeamName { get; set; } = string.Empty;
        [DisplayName("Land")]
        public string CountryNameShort { get; set; } = string.Empty;
        [DisplayName("Aantal renners")]
        public int? CompetitorsInTeam { get; set; }
        public IEnumerable<CompetitorViewModel> Competitors { get; set; } = Enumerable.Empty<CompetitorViewModel>();
    }
}
