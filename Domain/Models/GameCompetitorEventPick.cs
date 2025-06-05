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
        public int GameCompetitorEventId { get; set; }
        public int CompetitorsInEventId { get; set; }

        public GameCompetitorEvent GameCompetitorEvent { get; set; }
        public CompetitorsInEvent CompetitorsInEvent { get; set;}
    }
}
