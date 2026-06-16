namespace CycleManager.Domain.Dto
{
    public class PickDetailDto
    {
        public int CompetitorInEventId { get; set; }
        public string CompetitorName { get; set; } = "";
        public int TotalScore { get; set; }
        public int LastScore { get; set; }
    }
}
