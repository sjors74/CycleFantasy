using Domain.Dto;

namespace WebApp.Pages.ViewModels
{
    public class EventListViewModel
    {
        public string Titel { get; set; } = string.Empty;
        public List<EventForUserDto> AllEvents { get; set; } = new();
        public string InlineText { get; set; } = string.Empty;
        public List<EventForUserDto> Evenementen { get; set; } = new();
        public bool ShowLinkToevoegen { get; set; } = true;

        public int CurrentEventId { get; set; }
    }
}
