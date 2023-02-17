using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace WebCycleManager.Models
{
    public class ResultItemViewModel
    {
        public int Id { get; set; }
        [DisplayName("Positie")]
        public int Position { get; set; }
        public int SelectedCompetitorId { get; set; }
        public IEnumerable<SelectListItem> DropdownList { get; set; }
        [DisplayName("Deelnemer")]
        public string CompetitorName { get; set; } = string.Empty;
        public int StageId { get; set; }
    }
}
