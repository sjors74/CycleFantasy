using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebCycleManager.Models
{
    public class CreateCompetitorViewModel
    {
        public int CompetitorId { get; set; }
        [DisplayName("Voornaam")]
        public string? FirstName { get; set; }
        [DisplayName("Achternaam")]
        public string? LastName { get; set; }
        [DisplayName("PCS-naam")]
        public string? PcsName { get; set; }
        [DisplayName("Land")]
        public int CountryId { get; set; }
        [DisplayName("Team")]
        [Required(ErrorMessage = "Kies een team.")]
        public int? TeamYearId { get; set; }
        [DisplayName("Kampioen")]
        public bool IsNationalChampion { get; set; }
        public int SeasonYearId { get; set; }
        public int SeasonYear { get; set; } 

        public List<SelectListItem> Teams { get; set; } = [];

        public List<SelectListItem> Countries { get; set; } = [];
    }
}
