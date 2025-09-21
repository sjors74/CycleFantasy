using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebCycleManager.Models
{
    public class CreateCompetitorViewModel
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string PcsName { get; set; } = string.Empty;

        [DisplayName("Land")]
        public int CountryId { get; set; }

        [DisplayName("Team")]
        public int TeamId { get; set; }

        [DisplayName("Kampioen")]
        public bool IsNationalChampion { get; set; }

        [DisplayName("Jaar")]
        public int Year { get; set; } = DateTime.Now.Year; // standaard huidig jaar
    }
}
