using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebCycleManager.Models
{
    public class RatingsIndexViewModel
    {
        public List<CompetitorRatingRowViewModel> Ratings { get; set; } = new();
        public string? Search { get; set; }
        public int? RatingCategoryId { get; set; }
        public List<SelectListItem> Categories { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; } 
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    }
}
