using DataAccessEF.Migrations;
using Domain.Models;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace WebCycleManager.Models
{
    public class GameCompetitorInEventViewModel
    {
        public int EventId { get; set; }
        public List<Competitor> Competitors { get; set; } = new List<Competitor>();
        public int GameCompetitorInEventPickId { get; set; }
        public int ConfigurationItems { get; set; }
        
        public virtual Event? Event { get; set; }
        
        public List<GameCompetitorInEventItemViewModel> Picks { get; set; }

        public GameCompetitorInEventViewModel(int eventId, Event? currentEvent, int gameCompetitorId, List<GameCompetitorInEventItemViewModel> picks, int configurationItems)
        {
            EventId = eventId;
            Event = currentEvent;
            GameCompetitorInEventPickId = gameCompetitorId;
            Picks = picks;
            ConfigurationItems = configurationItems;
        }

    }
}
