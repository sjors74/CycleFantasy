using System.ComponentModel.DataAnnotations;

namespace CycleManager.Domain.Models
{
    public class RatingCategory
    {
        [Key]
        public int RatingCategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
