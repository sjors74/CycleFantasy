using Domain.Dto;

namespace CycleManager.Domain.Dto
{
    public class TeamDto
    {
        public int Id { get; set; }
        public required string Naam { get; set; }
        public required List<CompetitorDto> Renners { get; set; }
    }
}
