using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class CompetitorsInEvent
    {
        [Key]
        public int Id { get; set; }
        public int EventId { get; set; }
        [NotMapped]
        public string FilterTeam { get; set; } = string.Empty;
        public int EventNumber { get; set; }
        public virtual Event? Event { get; set; }
        public int CompetitorId { get; set; }
        public bool OutOfCompetition { get; set;} = false;
        public virtual Competitor? Competitor { get; set; }

    }
}
