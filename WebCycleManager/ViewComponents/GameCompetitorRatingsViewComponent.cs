using Microsoft.AspNetCore.Mvc;
using WebCycleManager.Models;

namespace WebCycleManager.ViewComponents
{
    public class GameCompetitorRatingsViewComponent : ViewComponent
    {
        private readonly int _maxRating;

        public GameCompetitorRatingsViewComponent(IConfiguration configuration)
        {
            if (!int.TryParse(configuration["ClientSettings:MaxRating"], out _maxRating))
            {
                _maxRating = 2500;
            }
        }

        public IViewComponentResult Invoke(IEnumerable<GameCompetitorRatingViewModel> ratings)
        {
            var model = new GameCompetitorRatingsViewModel
            {
                Ratings = ratings?.ToList() ?? new(),
                MaxRating = _maxRating
            };

            return View(model);
        }
    }
}
