using System.ComponentModel.DataAnnotations;

namespace CycleManager.Domain.Enums
{
    public enum QuestionType
    {
        [Display(Name = "Algemeen klassement")]
        GC = 0,
        [Display(Name = "Bergklassement")]
        KOM = 1,
        [Display(Name = "Puntenklassement")]
        Points = 2,
        [Display(Name = "Bonusvraag")]
        Bonus_CompetitorId = 3,
        [Display(Name = "Bonusvraag")]
        Bonus_Number = 4,
        [Display(Name = "Bonusvraag")]
        Bonus_String = 5
    }
}
