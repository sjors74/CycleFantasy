using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class SpecialResult
    {
        [Key]
        public int Id { get; set; }

        public int StageId { get; set; }
        public Stage? Stage { get; set; }


        public int CompetitorInEventId { get; set; }
        public CompetitorsInEvent? CompetitorInEvent { get; set; }
        
        public int? SpecialId { get; set; }
        public ConfigurationItemSpecial? Special { get; set; }
        
    }
}
