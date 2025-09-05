namespace CycleManager.Domain.Dto
{
    public class StageResultDto
    {
        public int StageId { get; set; }
        public int StageNumber { get; set; }
        public bool HasResult { get; set; }
        public string VanNaar { get; set; } = string.Empty;
        public bool NoScore { get; set; }
    }
}
