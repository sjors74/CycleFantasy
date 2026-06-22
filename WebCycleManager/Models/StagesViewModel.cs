using CycleManager.Domain.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace WebCycleManager.Models
{
    public class StagesViewModel
    {
        public List<StageViewModel> Stages { get; set; }
        public int CurrentSearchEventId { get; set; }
        public List<SelectListItem>? Events { get; set; }
        public StagesViewModel()
        {
            Stages = new List<StageViewModel>();

        }
    }

    public class StageViewModel
    {
        public int StageId { get; set; }
        [DisplayName("Etappe")]
        public string StageName { get; set; } = string.Empty;
        public int StageOrder { get; set; }
        [DisplayName("Datum")]
        public DateOnly StageDate { get; set; }
        [DisplayName("Start locatie")]
        public string StartLocation { get; set; } = string.Empty;
        [DisplayName("Finish locatie")]
        public string FinishLocation { get; set; } = string.Empty;
        public int EventId { get; set; }
        [DisplayName("Evenement")]
        public string EventName { get; set; } = string.Empty;
        [DisplayName("Jaar")]
        public int EventYear { get; set; }
        public int AantalPosities { get; set; }
        public string StageNameLong
        { 
            get
            {
                return $"Etappe {StageName}: {StartLocation}-{FinishLocation}";
            }
        }
        [DisplayName("Geen uitslag")]
        public bool NoScore { get; set; }
        [DisplayName("Opmerking")]
        public string? NoScoreDescription { get; set; }
        public DateOnly EventStartDate { get; set; }
        public DateOnly EventEndDate { get; set; }
        public string StageDateMin => EventStartDate.ToString("yyyy-MM-dd");
        public string StageDateMax => EventEndDate.ToString("yyyy-MM-dd");

        public ScrapeStatus ScrapeStatus { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> AvailableStatuses { get; set; } = [];
    }
}
