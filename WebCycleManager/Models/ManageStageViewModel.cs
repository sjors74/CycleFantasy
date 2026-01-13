using System.ComponentModel.DataAnnotations;

namespace WebCycleManager.Models
{
    public class ManageStageViewModel
    {
        [Required]
        public StageCreateViewModel NewStage { get; set; } = new();

        public EventStagesViewModel EventStages { get; set; } = new ();

        public string? UiErrorMessage { get; set; }
    }
}
