using Domain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace WebCycleManager.Models
{
    public class CompetitorsInEventViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int EventYear { get; set; }
        [DisplayName("Deelnemers")]
        public List<CompetitorInEventViewModel> Competitors { get; set; }
        public IEnumerable<SelectListItem> Teams { get; set; }
        public string FilterTeam { get; set; }
        public string EventNameLong
        {
            get
            {
                return $"{EventName} {EventYear}";
            }
        }

        public CompetitorsInEventViewModel(List<CompetitorInEventViewModel> competitors, string eventName, int eventYear, int eventId)
        {
            Competitors = competitors;
            EventName = eventName;
            EventYear = eventYear;
            EventId = eventId;
        }
    }
}
