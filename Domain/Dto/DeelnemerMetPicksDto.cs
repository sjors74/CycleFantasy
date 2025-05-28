namespace CycleManager.Domain.Dto
{
    public class DeelnemerMetPicksDto
    {
        public int Id { get; set; }
        public string PoolNaam { get; set; } = string.Empty;
        public string DeelnemerNaam { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public List<ResultDto> Picks { get; set; } = [];
    }
}
