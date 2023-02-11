using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Result
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int StageId { get; set; }
        public int CompetitorId { get; set; }
        public virtual Stage? Stage { get; set; }
        public virtual Competitor? Competitor { get;set; }
        public virtual ConfigurationItem? ConfigurationItem { get; set; }
    }
}
