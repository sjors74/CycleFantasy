using Domain.Dto;

namespace CycleManager.Domain.Dto
{
    public class EventDashboardDto
    {
        public string? Titel { get; set; }
        public List<EventForUserDto> Actueel { get; set; } = new();
        public List<EventForUserDto> Toekomst { get; set; } = new();
        public List<EventForUserDto> Historisch { get; set; } = new();
    }
}
