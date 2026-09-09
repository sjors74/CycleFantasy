using Microsoft.AspNetCore.Mvc;
using WebCycleManager.Models;

namespace WebCycleManager.ViewComponents
{
    public class CompetitorRatingsViewComponent : ViewComponent
    {
        private readonly int _maxRating;

        public CompetitorRatingsViewComponent(IConfiguration configuration)
        {
            if (!int.TryParse(
                    configuration["ClientSettings:MaxRating"],
                    out _maxRating))
            {
                _maxRating = 2500;
            }
        }

        public IViewComponentResult Invoke(
            IEnumerable<CompetitorRatingViewModel> ratings)
        {
            var model = new CompetitorRatingsViewModel
            {
                Ratings = ratings?
                    .OrderBy(r => r.DisplayOrder)
                    .ToList() ?? new(),

                MaxRating = _maxRating
            };

            return View(model);
        }
    }
}
