namespace WebCycleManager.Models
{
    public class EventTeamsViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public List<TeamSelection> Teams { get; set; } = new();
    }

    public class TeamSelection
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}