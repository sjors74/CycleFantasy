using Domain.Dto;

namespace CycleManager.Domain.Dto
{
    public class EventViewDto
    {
        public List<EventForUserDto> ActieveEvenementen { get; set; } = [];
        public List<EventForUserDto> ToekomstigeEvenementen { get; set; } = [];
    }
}