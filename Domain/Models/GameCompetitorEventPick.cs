using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class GameCompetitorEventPick
    {
        [Required]
        public int Id { get; set; }
        public int GameCompetitorEventId { get; set; }
        public int CompetitorsInEventId { get; set; }

        public GameCompetitorEvent GameCompetitorEvent { get; set; } = null!;
        public CompetitorsInEvent CompetitorsInEvent { get; set;} = null!;
    }
}
