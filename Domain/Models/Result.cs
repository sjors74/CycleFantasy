using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Result
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int StageId { get; set; }
        public int CompetitorInEventId { get; set; }
        public int ConfigurationItemId { get; set; }
        [JsonIgnore]
        public virtual Stage? Stage { get; set; }
        [JsonIgnore]
        public virtual CompetitorsInEvent? CompetitorInEvent { get;set; }
        [JsonIgnore]
        public virtual ConfigurationItem? ConfigurationItem { get; set; }
        
    }
}
