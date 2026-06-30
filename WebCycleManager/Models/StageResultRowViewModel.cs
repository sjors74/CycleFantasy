namespace WebCycleManager.Models
{
    public class StageResultRowViewModel
    {
        public bool IsSpecial { get; set; }

        // Result
        public int? Position { get; set; }

        // Special
        public int? SpecialId { get; set; }
        public string? SpecialName { get; set; }

        // Gemeenschappelijk
        public int Id { get; set; }                    // Result.Id of SpecialResult.Id
        public int SelectedCompetitorId { get; set; }
        public string CompetitorName { get; set; } = string.Empty;

        public string Label =>
            IsSpecial
                ? SpecialName ?? string.Empty
                : Position?.ToString() ?? string.Empty;

        public string DeleteAction =>
            IsSpecial ? "DeleteSpecial" : "Delete";
    }
}
