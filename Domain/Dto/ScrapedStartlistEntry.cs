namespace CycleManager.Domain.Dto
{
    public class ScrapedStartlistEntry
    {
        public string RiderName { get; set; } = "";
        public string PcsName { get; set; } = "";
        public string TeamName { get; set; } = "";
        public string TeamPcsName { get; set; } = "";
        public int? BibNumber { get; set; }
    }
}
