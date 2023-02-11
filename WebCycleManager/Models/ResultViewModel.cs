namespace WebCycleManager.Models
{
    public class ResultViewModel
    {
        public int StageId { get; set; }
        public string StageName { get; set; } = string.Empty;

        public IEnumerable<ResultItemViewModel> Results { get; set; }
    }
}
