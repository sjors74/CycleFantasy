using CycleManager.Domain.Dto;

namespace Domain.Dto
{
    public class EventForUserDto: EventDto
    {
        public string UserId { get; set; } = string.Empty;
        public int CompetitorInEventId { get; set; }
        public List<ResultDto> Renners { get; set; } = [];
    }
}
