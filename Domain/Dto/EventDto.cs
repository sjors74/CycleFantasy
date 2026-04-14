using CycleManager.Domain.Dto;

namespace Domain.Dto
{
    public class EventDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set;}
        public string? Slogan { get; set; }
        public string? CountryCode { get; set; }
        public string? ColorName { get; set; }
        public bool ShowPodium { get; set; }
        public List<StageResultDto>? Stages { get; set; }
        public List<DeelnemerDto>? Deelnemers { get; set; } = new();
        public bool IsActive { get; set; }

    }
}
