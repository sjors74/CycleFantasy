using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebCycleManager.Models
{
    public class PickDetailViewModel
    {
        public int CompetitorInEventId { get; set; }
        public string CompetitorName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsOutOfCompetition { get; set; }
        public int Score { get; set; }
        public int PickId { get; set; }
        public int? SelectedCompetitorId { get; set; }
        public IEnumerable<SelectListItem> Competitors { get; set; } = new List<SelectListItem>();
    }
}
