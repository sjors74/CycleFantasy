namespace CycleManager.Domain.ViewModel
{
    public class StageViewModel
    {
        public int StageId { get; set; }
        public string StageName { get; set; } = string.Empty;
        public int StageOrder { get; set; }
        public string StartLocation { get; set; } = string.Empty;
        public string FinishLocation { get; set; } = string.Empty;
        public int AantalPosities { get; set; }
        public bool NoScore { get; set; }
    }
}
