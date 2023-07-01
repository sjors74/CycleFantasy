namespace WebCycleManager.Models
{
    public class GameCompetitorViewModel
    {
        public int GameCompetitorId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string GameCompetitorName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }
    }
}
