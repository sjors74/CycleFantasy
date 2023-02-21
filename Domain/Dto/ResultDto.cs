namespace Domain.Dto
{
    public class ResultDto
    {
        public string StageNumber { get; set; } = string.Empty;
        public string CompetitorName { get; set; } = string.Empty;
        public int Position { get; set; }
        public int Points { get; set; }

        public ResultDto()
        {

        }
    }
}
