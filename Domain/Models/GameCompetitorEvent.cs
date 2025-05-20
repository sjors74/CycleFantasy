using CycleManager.Domain.Models;
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
        public string? UserId { get; set; } = string.Empty;
        public virtual ApplicationUser? User { get; set; }
        public int EventId { get; set; }
        public virtual Event? Event { get; set; }
    }
}
