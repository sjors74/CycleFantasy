namespace CycleManager.Domain.ViewModel
{
    public class EventDetailsViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string EventCode { get; set; } = string.Empty;
        public string? Slogan { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<StageViewModel> Stages { get; set; } = new();

        public int StagesInEvent => Stages.Count;
    }
}
