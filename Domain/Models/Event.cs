using CycleManager.Domain.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
        [DisplayName("Code")]
        public string EventCode { get; set; } = string.Empty;
        [DisplayName("Startdatum")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }
        [DisplayName("Einddatum")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
        public string? Slogan { get; set; }
        [DisplayName("Landcode")]
        public string? CountryCode { get; set; }
        [DisplayName("Kleur")]
        public string? ColorName { get; set; }
        public bool IsActive { get; set; } = false;
        public bool ShowPodium { get; set; } = false;
        public int? ConfigurationId { get; set; }
        [JsonIgnore]
        public virtual Configuration? Configuration {get;set;}
        public virtual ICollection<Stage> Stages { get; set; } = [];
        public virtual ICollection<GameCompetitorEvent> GameCompetitorEvents { get; set; } = [];
        public virtual ICollection<CompetitorsInEvent> CompetitorsInEvent { get; set; } = [];
        public ICollection<EventTeam>? EventTeams { get; set; }
    }
}
