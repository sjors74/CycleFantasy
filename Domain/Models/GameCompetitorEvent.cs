using CycleManager.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    [Index(nameof(EventId), nameof(UserId), nameof(TeamName), IsUnique = true)]
    public class GameCompetitorEvent
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Teamnaam")]
        public string TeamName { get; set; } = string.Empty;
        public string UserId { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
        public int EventId { get; set; }
        public virtual Event Event { get; set; } = null!;
        public virtual ICollection<GameCompetitorEventPick> Renners { get; set; } = new List<GameCompetitorEventPick>();

    }
}
