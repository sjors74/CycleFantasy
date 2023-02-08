using System.ComponentModel.DataAnnotations;

namespace WebCycle.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }
        public string EventName { get; set; }
        public int EventYear { get; set; }  
    }
}
