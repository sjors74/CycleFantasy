using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class CompetitorDto
    {
        public int CompetitorId { get; set; }
        public string CompetitorName { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string CountryShort { get; set; } = string.Empty;
        public string EventNumber { get; set; } = string.Empty;
    }
}
