namespace CycleManager.Domain.Models
{
    /// <summary>
    /// resultaatmodel, geen db tabel!
    /// </summary>
    public class DeelnemerScoreBreakdown
    {
        public int GameCompetitorEventId { get; set; }

        public int NormalPoints { get; set; }

        public int SpecialPoints { get; set; }

        public int TotalPoints => NormalPoints + SpecialPoints;
    }
}
