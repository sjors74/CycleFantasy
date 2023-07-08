using Domain.Models;

namespace WebCycleManager.Models
{
    public class GameCompetitorInEventViewModel
    {
        public int Id { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string GameCompetitorName { get; set; } = string.Empty;
        public int EventId { get; set; }
        public List<ResultLineViewModel> CompetitorsInEvent { get; set; } = new List<ResultLineViewModel>();
        public int GameCompetitorInEventId { get; set; }
        public int Score { get; set; } = 0;
    }
}
