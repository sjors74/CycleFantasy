using Domain.Models;

namespace CycleManager.Domain.Dto
{
    public class ResultDto
    {
        public string StageNumber { get; set; } = string.Empty;
        public string CompetitorName { get; set; } = string.Empty;
        public string CompetitorTeam { get; set; } = string.Empty;
        public int Position { get; set; }
        public int Points { get; set; }
        public int LatestPoints { get; set; }
        public string PcsName { get; set; }
        public bool IsNationalChampion { get; set; }
        public List<ConfigurationItem> ConfigurationItems { get; set; } = [];
        public int EventId { get; set; }
        public int CompetitorInEventId { get; set; }
        public bool OutOfCompetition { get; set; }
        public string? CountryCode { get; set; }
        public ResultDto()
        {

        }
    }
}
