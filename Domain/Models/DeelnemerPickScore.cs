using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class DeelnemerPickScore
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public int GameCompetitorEventPickId { get; set; }

        public int? StageId { get; set; }

        public int Score { get; set; }

        public DateTime LastUpdate { get;set; }

        public virtual GameCompetitorEventPick Pick { get; set; }

        public virtual Stage Stage { get; set; }
    }
}
