namespace CycleManager.Domain.Models
{
    public class SeasonYear
    {
        public int SeasonYearId { get; set; }

        public int Year { get; set; }   // 2026

        public bool Active { get;set; } = true;
    }
}
