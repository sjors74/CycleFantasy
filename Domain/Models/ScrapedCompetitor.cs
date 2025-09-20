namespace CycleManager.Domain.Models
{
    public class ScrapedCompetitor
    {
        public int Id { get; set; }
        public string RiderName { get; set; } = "";
        public int TeamId { get; set; }
        public int Year { get; set; }
        public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
    }
}
