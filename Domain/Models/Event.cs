using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int EventYear { get; set; }
    }
}
