using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class Team
    {
        [Key]
        public int TeamId { get; set; }
        [DisplayName("Team naam")]
        public string TeamName { get; set; } = string.Empty;
        [DisplayName("Land")]
        public int? CountryId { get; set; }
        [NotMapped]
        public virtual Country? Country { get; set; }
        
        public virtual ICollection<Competitor>? Competitors { get; set;}
    }
}
