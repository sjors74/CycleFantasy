using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebCycleManager.Models
{
    public class CompetitorViewModel
    {
        public int CompetitorId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string Land { get; set; } = string.Empty;

        public string CompetitorName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }
    }
    public class CompetitorInEventViewModel : CompetitorViewModel
    {
        public int CompetitorInEventId { get; set; }
        [DisplayName("Rugnummer")]
        public int EventNumber { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int TeamId { get; set; }
    }

}
