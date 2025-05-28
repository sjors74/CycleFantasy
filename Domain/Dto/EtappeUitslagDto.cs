namespace CycleManager.Domain.Dto
{
    public class EtappeUitslagDto
    {
        public int Positie { get; set; } 
        public string CompetitorName { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public int Score { get;set ; }
    }
}
