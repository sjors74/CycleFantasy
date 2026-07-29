namespace CycleManager.Domain.Dto
{
    public class RatingCategoryDto
    {
        public int RatingCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Color { get; set; } = "secondary";
        public int DisplayOrder { get; set; }
    }
}
