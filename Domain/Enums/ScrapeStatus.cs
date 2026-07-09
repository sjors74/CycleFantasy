using System.ComponentModel.DataAnnotations;

namespace CycleManager.Domain.Enums
{
    public enum ScrapeStatus
    {
        [Display(Name = "in behandeling")]
        Pending = 0,
        [Display(Name = "gedeeltelijk")]
        Partial = 1,
        [Display(Name = "voltooid")]
        Completed = 2,
        [Display(Name = "mislukt")]
        Failed = 3,
        [Display(Name = "overgeslagen")]
        Skipped = 4
    }
}
