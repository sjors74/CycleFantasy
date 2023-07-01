using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class GameCompetitorEvent
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Teamnaam")]
        public string TeamName { get; set; } = string.Empty;

        public int GameCompetitorId { get; set; }
        public virtual GameCompetitor? GameCompetitor { get; set; }
        public int EventId { get; set; }
        public Event? Event { get; set; }
    }
}
