using CycleManager.Domain.Dto;
using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels;

namespace WebApp.ViewComponents
{
    public class CompetitorRatingsViewComponent : ViewComponent
    {
        private readonly int _maxRating;
        public CompetitorRatingsViewComponent(IConfiguration configuration)
        {
            if (!int.TryParse(configuration["ClientSettings:MaxRating"], out _maxRating))
            {
                _maxRating = 2500; // fallback
            }
        }
        public IViewComponentResult Invoke(IEnumerable<CompetitorRatingDto> ratings) 
        { 
            var model = new CompetitorRatingsViewModel 
            { 
                Ratings = ratings?.OrderBy(r => r.RatingCategoryId).ToList() ?? new(), 
                MaxRating = _maxRating
            }; 

            return View(model); 
        }
    }
}