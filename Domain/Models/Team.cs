using CycleManager.Domain.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class Team
    {
        [Key]
        public int TeamId { get; set; }
        [DisplayName("Team naam")]
        public string CurrentTeamName { get; set; } = string.Empty;
        [DisplayName("Land")]
        public int? CountryId { get; set; }
        public virtual Country? Country { get; set; }
        public string? PcsName { get; set; } = string.Empty;
        public virtual ICollection<CompetitorInTeam> CompetitorInTeams { get; set; } = [];
        public ICollection<TeamYear> TeamYears { get; set; } = [];
        public ICollection<EventTeam> EventTeams { get; set; } = [];
    }
}
