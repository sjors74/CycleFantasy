namespace CycleManager.Domain.Dto
{
    public class DeelnemerDeleteDto
    {
        public int Id { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public int EventId { get; set; }
    }

}
