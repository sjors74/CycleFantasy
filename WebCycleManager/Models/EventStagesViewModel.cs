using Domain.Models;

namespace WebCycleManager.Models
{
    public class EventStagesViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventStartDate { get; set; }
        public DateTime EventEndDate { get; set; }
        public IEnumerable<Stage>? Stages { get; set; }
    }
}