using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class CompetitorsInEvent
    {
        [Key]
        public int CompetitorInEventId { get; set; }
        public int EventId { get; set; }

        public int EventNumber { get; set; }
        public virtual Event Event { get; set; }
        public int CompetitorId { get; set; }
        public virtual Competitor Competitor { get; set; }
    }
}
