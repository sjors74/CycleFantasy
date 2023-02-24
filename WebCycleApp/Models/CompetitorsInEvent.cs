using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCycleApp.Models
{
    public class CompetitorsInEvent
    {
        public int CompetitorId { get; set; }
        public string CompetitorName { get; set; }
        public int CountryId { get; set; }
        public string CountryNameShort { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set;}
    }
}
