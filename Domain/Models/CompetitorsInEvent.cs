using CycleManager.Domain.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class CompetitorsInEvent
    {
        [Key]
        public int Id { get; set; }
        public int CompetitorInTeamId { get; set; }
        public CompetitorInTeam CompetitorInTeam { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }

        [NotMapped]
        public string FilterTeam { get; set; } = string.Empty;
        public int EventNumber { get; set; }
        public bool OutOfCompetition { get; set;} = false;
        public bool InSelectie { get; set; } = false;
        public bool RemovedFromStartList { get; set; }

        public virtual ICollection<GameCompetitorEventPick> GameCompetitorEventPicks { get; set; } = [];

    }
}
