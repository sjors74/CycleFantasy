using System.ComponentModel.DataAnnotations;

namespace CycleManager.Domain.Dto
{
    public class DeelnemerCreateDto
    {
        [Required]
        public string TeamName { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int EventId { get; set; }
    }
}
