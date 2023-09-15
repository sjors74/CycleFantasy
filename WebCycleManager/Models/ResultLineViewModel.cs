using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebCycleManager.Models
{
    public class ResultLineViewModel
    { 
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Score { get; set; }
        public bool OutOfCompetition { get; set; } = false;
        public int CompetitorInEventId { get; set; }
        public int EventId { get; set; }
        public int SelectedCompetitorId { get; set; }
        public IEnumerable<SelectListItem> DropdownList { get; set; }

    }
}
