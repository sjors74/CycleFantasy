using Domain.Models;

namespace WebCycleManager.Models
{
    public class EventViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StagesInEvent { get; set; }
        public IEnumerable<Stage>? Stages { get; set; }
    }
}
