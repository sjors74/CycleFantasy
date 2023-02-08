using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
        [DisplayName("Team")]
        public int TeamId { get; set; }
        [DisplayName("Land")]
        public int CountryId { get; set; }
        [NotMapped]
        public virtual Team? Team { get; set; }
        [NotMapped]
        public virtual Country? Country { get; set; }
    }
}
