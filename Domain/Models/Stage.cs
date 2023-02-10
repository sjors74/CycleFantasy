using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class Stage
    {
        [Key]
        public int Id { get; set; }
        [DisplayName("Etappe")]
        public string StageName { get; set; } = string.Empty;
        public int StageOrder { get; set; }
        [DisplayName("Start locatie")]
        public string StartLocation { get; set;} = string.Empty;
        [DisplayName("Finish locatie")]
        public string FinishLocation { get; set; } = string.Empty;
        [DisplayName("Evenement")]
        public int EventId { get; set; }
        [NotMapped]
        public virtual Event? Event{ get; set; }

    }
}
