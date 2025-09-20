using CycleManager.Domain.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class Competitor
    {
        [Key]
        public int CompetitorId { get; set; }
        
        [DisplayName("Voornaam")]
        public string FirstName { get; set; } = string.Empty;
        [DisplayName("Achternaam")]
        public string LastName { get; set; } = string.Empty;
        public string PcsName { get; set; } = string.Empty;

        [DisplayName("Land")]
        public int CountryId { get; set; }
        [DisplayName("Land")]
        public Country? Country { get; set; }
        public virtual ICollection<CompetitorInTeam> CompetitorInTeams { get; set; } = [];

        [NotMapped]
        [DisplayName("Naam")]
        public string CompetitorName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }
    }
}
