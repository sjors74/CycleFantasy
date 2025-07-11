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
        
        public virtual Event Event{ get; set; } 
        public virtual ICollection<Result> Results { get; set; } = [];
    }
}
