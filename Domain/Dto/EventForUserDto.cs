using CycleManager.Domain.Dto;
using Domain.Models;
using System.ComponentModel;

namespace Domain.Dto
{
    public class EventForUserDto: EventDto
    {
        public string UserId { get; set; } = string.Empty;
        public int CompetitorInEventId { get; set; }
        public List<ResultDto> Renners { get; set; } = [];
        public bool IsIngeschreven { get; set; }
        public bool CanSubscribe { get; set; }

        public bool CanCreatePool { get; set; }

        public bool IsReadOnly { get; set; }
    }
}
