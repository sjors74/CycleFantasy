using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebCycleManager.Models
{
    public class StageCreateViewModel
    {
        public int? StageId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Datum")]
        public DateTime StageDate { get; set; }

        [Required(ErrorMessage = "De etappenaam is verplicht.")]
        [Display(Name = "Etappe naam")]
        [StringLength(100)]
        public string StageName { get; set; } = string.Empty;

        public int StageOrder { get; set; }

        [StringLength(50)]
        [Display(Name = "Van")]
        public string StartLocation { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Naar")]
        public string FinishLocation { get; set; } = string.Empty;

        [Display(Name = "Geen score")]
        public bool NoScore { get; set; }

        [Display(Name = "Omschrijving geen score")]
        public string? NoScoreDescription { get; set; }

        [Required]
        [Display(Name ="Evenement")]
        public int EventId { get; set; }

        public IEnumerable<SelectListItem>? Events { get; set; }
    }
}
