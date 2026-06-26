using CycleManager.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class Stage
    {
        [Key]
        public int Id { get; set; }
        [DisplayName("Datum")]
        public DateTime StageDate { get; set; }
        [DisplayName("Etappe")]
        public string StageName { get; set; } = string.Empty;
        public int StageOrder { get; set; }
        [DisplayName("Start locatie")]
        public string StartLocation { get; set;} = string.Empty;
        [DisplayName("Finish locatie")]
        public string FinishLocation { get; set; } = string.Empty;
        [DisplayName("Evenement")]
        public int EventId { get; set; }
        public bool NoScore { get; set; }
        public string? NoScoreDescription { get; set; }
        public ScrapeStatus ScrapeStatus { get; set; }
        public DateTime? LastScrapeAttempt { get; set; }
        public DateTime? LastSuccessfulScrape { get; set; }
        public virtual Event Event{ get; set; } 
        public virtual ICollection<Result> Results { get; set; } = [];
    }
}
