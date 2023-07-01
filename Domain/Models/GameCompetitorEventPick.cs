using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class GameCompetitorEventPick
    {
        [Key]
        public int Id { get; set; } 
        public int GameCompetitorEventId { get; set; }
        //TODO: Should be a reference to GameCompetitorEvent-object, to establish a FK-relation.
        //public GameCompetitorEvent GameCompetitorEvent { get; set; }
        public virtual IEnumerable<CompetitorsInEvent> CompetitorsInEvent { get; set; } = new List<CompetitorsInEvent>();
    }
}
