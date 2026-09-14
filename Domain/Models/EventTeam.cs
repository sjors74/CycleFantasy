using Domain.Models;

namespace CycleManager.Domain.Models
{
    public class EventTeam
    {
        public int EventId { get; set; }
        public Event Event {  get; set; } = null!;

        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;
    }
}
