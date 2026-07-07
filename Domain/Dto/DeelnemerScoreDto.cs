namespace CycleManager.Domain.Dto
{
    public class DeelnemerScoreDto
    {
        public int GameCompetitorEventId { get; set; }

        public int NormalScore { get; set; }

        public int SpecialScore { get; set; }

        public int TotalPoints => NormalScore + SpecialScore;
    }
}
