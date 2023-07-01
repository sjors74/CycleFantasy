using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Models
{
    public class CompetitorsInEvent
    {
        [Key]
        public int CompetitorInEventId { get; set; }
        public int EventId { get; set; }
        [NotMapped]
        public string FilterTeam { get; set; } = string.Empty;
        public int EventNumber { get; set; }
        public virtual Event? Event { get; set; }
        public int CompetitorId { get; set; }
        
        public virtual Competitor? Competitor { get; set; }
        public List<GameCompetitorEventPick> GameCompetitorEventPicks { get; set; } = new();
    }
}
