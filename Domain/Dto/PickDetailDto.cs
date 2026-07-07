namespace CycleManager.Domain.Dto
{
    public class PickDetailDto
    {
        public int CompetitorInEventId { get; set; }
        public string CompetitorName { get; set; } = "";
        public int NormalScore { get; set; }
        public int SpecialScore { get; set; }
        public int TotalScore { get; set; }
        public int LastScore { get; set; }
        public List<SpecialDetailDto> Specials { get; set; } = [];
    }

}
