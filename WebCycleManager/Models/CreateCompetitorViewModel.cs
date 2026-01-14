using System.ComponentModel;

namespace WebCycleManager.Models
{
    public class CreateCompetitorViewModel
    {
        public int CompetitorId { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? PcsName { get; set; }

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
