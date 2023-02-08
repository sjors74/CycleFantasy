using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class Country
    {
        [Key]
        public int CountryId { get; set; }
        [DisplayName("Land")]
        public string CountryNameLong { get; set; } = string.Empty;
        [DisplayName("Afkorting")]
        public string CountryNameShort { get; set;} = string.Empty;

    }
}
