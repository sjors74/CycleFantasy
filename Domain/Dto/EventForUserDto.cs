using CycleManager.Domain.Dto;

namespace Domain.Dto
{
    public class EventForUserDto: EventDto
    {
        public string UserId { get; set; } = string.Empty;
        public int CompetitorInEventId { get; set; }
        public List<CompetitorRankingDto> Renners { get; set; } = [];
        public bool IsIngeschreven { get; set; }
        public bool CanSubscribe { get; set; }

        public bool CanCreatePool => CanSubscribe;

        public bool IsReadOnly => !CanSubscribe;

        public string? Category
        {
            get
            {
                var now = DateTime.UtcNow;
                var isFuture = StartDate > now;

                if (isFuture)
                    return "toekomst";

                if (IsActive)
                    return "actueel";

                return "historisch";
            }
        }
    }
}
