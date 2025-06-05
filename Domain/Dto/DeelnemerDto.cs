using Domain.Dto;
using Domain.Models;

namespace CycleManager.Domain.Dto
{
    public class DeelnemerDto
    {
        public int Id { get; set; }
        public string PoolNaam { get; set; } = string.Empty;
        public string DeelnemerNaam { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public List<CompetitorDto> Renners { get; set; } = [];
        public int Punten { get; set;}
        public int EventId { get; set; }
    }
}
