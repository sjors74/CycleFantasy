using System.ComponentModel.DataAnnotations;

namespace CycleManager.Domain.Dto
{
    public class DeelnemerEditDto
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Teamnaam")]
        public string TeamName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Gebruiker")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int EventId { get; set; }
    }
}
