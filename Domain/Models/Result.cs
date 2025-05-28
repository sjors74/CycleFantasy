using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Models
{
    public class Result
    {
        [Key]
        public int Id { get; set; }
        [Required]

        public int StageId { get; set; }
        [JsonIgnore]
        public virtual Stage? Stage { get; set; }


        public int CompetitorInEventId { get; set; }
        [JsonIgnore]
        public virtual CompetitorsInEvent? CompetitorInEvent { get; set; }
        
        public int ConfigurationItemId { get; set; }
        [JsonIgnore]
        public virtual ConfigurationItem? ConfigurationItem { get; set; }
        
    }
}
