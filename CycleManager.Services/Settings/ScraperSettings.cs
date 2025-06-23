namespace CycleManager.Services.Settings
{
    public class ScraperSettings
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Stage { get; set; }
        public int TopLimit { get; set; }
    }
}
