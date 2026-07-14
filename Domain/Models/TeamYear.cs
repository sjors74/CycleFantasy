using Domain.Models;

namespace CycleManager.Domain.Models
{
    public class TeamYear
    {
        public int TeamYearId { get; set; }
        
        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;
        
        public int Year { get; set; }

        public int SeasonYearId { get; set; }
        public SeasonYear SeasonYear { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
    }
}
