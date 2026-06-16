using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCycleApp.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int EventYear { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public Event(int eventId, string eventName, int eventYear, DateTime? startDate, DateTime? endDate)
        {
            EventId = eventId;
            EventName = eventName;
            EventYear = eventYear;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
