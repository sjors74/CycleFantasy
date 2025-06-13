namespace CycleManager.Domain.Dto
{
    public class SelectieDto
    {
        public Guid UserId { get; set; }
        public int DeelnemerId { get; set; } //GameCompetitorEventId.
        public List<int> RennerIds { get; set; } = []; //Lijst van CompetitorIds
        public int EventId { get; set; }
    }
}
