namespace WebCycleManager.Models
{
    public class StageDeleteViewModel
    {
        public int StageId { get; set; }
        public string StageName { get; set; } = string.Empty;
        public string StageDescription { get; set; } = string.Empty;
        public int EventId { get; set; }
    }
}
