using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class GameCompetitorEventPick
    {
        [Required]
        public int Id { get; set; }
//        [Required]
//        public int GameCompetitorEventId { get; set; }
//        [Required]
//        public int CompetitorInEventId { get; set; }

        public virtual GameCompetitorEvent GameCompetitorEvent { get; set; } = new GameCompetitorEvent();
        public virtual CompetitorsInEvent CompetitorsInEvent { get; set;} = new CompetitorsInEvent();
    }
}
