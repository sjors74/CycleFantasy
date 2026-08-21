namespace CycleManager.Domain.Dto
{
    public class SeasonYearDto
    {
        public int SeasonYearId { get; set; }
        public int Year { get; set; } = 0;
        public bool Active { get; set; } = false;
    }
}