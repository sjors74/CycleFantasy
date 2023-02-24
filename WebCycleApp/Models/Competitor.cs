using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCycleApp.Models
{
    public class Competitor
    {
        public int CompetitorId { get; set; }
        public string CompetitorName { get; set; }
        public int CountryId { get; set; }
        public int TeamId { get; set; }
    }
}
