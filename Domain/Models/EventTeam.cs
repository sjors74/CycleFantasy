using Domain.Models;

namespace CycleManager.Domain.Models
{
    public class EventTeam
    {
        public int EventId { get; set; }
        public required Event Event {  get; set; }
        
        public int TeamId { get; set; }
        public required Team Team { get; set; }
    }
}
