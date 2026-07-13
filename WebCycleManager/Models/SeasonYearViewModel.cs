using System.ComponentModel.DataAnnotations;

namespace WebCycleManager.Models
{
    public class SeasonYearViewModel
    {
        public int SeasonYearId { get; set; }
        [Display(Name = "Jaar")]
        [Required(ErrorMessage = "Vul een jaar in.")]
        [Range(2020,2150, ErrorMessage = "Voer een geldig jaar in.")]
        public int Year { get; set; }

        [Display(Name = "Actief")]
        public bool Active { get; set; } = true;
    }
}
