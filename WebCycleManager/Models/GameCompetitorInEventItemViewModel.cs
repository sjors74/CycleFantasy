using Domain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCycleManager.Models
{
    public class GameCompetitorInEventItemViewModel
    {
        public int GameCompetitorEventPickId { get; set; }
        public int EventId { get; set; }
        public int SelectedCompetitorId { get; set; }
        public int GameCompetitorEventId { get; set; }
        //public IEnumerable<SelectListItem> DropdownList { get; set; }
        public string CompetitorName { get; set; } = string.Empty;
        public string GameCompetitorName { get; set;} = string.Empty;
        [NotMapped]
        public int Position { get; set; }
        public int Score { get; set; }
    }
}
