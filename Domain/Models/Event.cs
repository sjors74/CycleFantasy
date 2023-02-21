using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Domain.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }
        [DisplayName("Evenement")]
        public string EventName { get; set; } = string.Empty;
        [DisplayName("Jaar")]
        public int EventYear { get; set; }
        [DisplayName("Startdatum")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }
        [DisplayName("Einddatum")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = false;
        public int? ConfigurationId { get; set; }
        public virtual Configuration? Configuration {get;set;}
    }
}
