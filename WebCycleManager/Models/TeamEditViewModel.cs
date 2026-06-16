using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebCycleManager.Models
{
    public class TeamEditViewModel
    {
        public int TeamId { get; set; }
        [Required]
        [Display(Name = "Huidige teamnaam")]
        public string CurrentTeamName { get; set; }= string.Empty;

        [Display(Name = "Land")]
        public int? CountryId { get; set; }
        public List<SelectListItem> Countries { get; set; } = [];

        [Display(Name = "PCS naam")]
        public string? PcsName { get; set; }

        public List<int> AvailableYears { get; set; } = [];

        public List<TeamYearViewModel> TeamYears { get; set; } = [];
    }

    public class TeamYearViewModel
    {
        public int TeamYearId { get; set; }
        public int Year { get; set; }
        [Display(Name = "Teamnaam")]
        public string Name { get; set; } = string.Empty;
    }
}
