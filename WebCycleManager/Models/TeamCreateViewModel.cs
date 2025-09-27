using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebCycleManager.Models
{
    public class TeamCreateViewModel
    {
        [Required]
        [Display(Name = "Teamnaam")]
        public string CurrentTeamName { get; set; } = string.Empty;
        [Display(Name = "PCS naam")]
        public string? PcsName { get; set; }

        [Required]
        [Display(Name = "Land")]
        public int CountryId { get; set; }

        public IEnumerable<SelectListItem> Countries { get; set; } = [];
    }
}
