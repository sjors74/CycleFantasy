using CycleManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // Add this at the top if not present
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class RatingController : Controller
    {
        private readonly IScraperService _scraperService;
        private readonly IRatingService _ratingService;

        public RatingController(IScraperService scraperService, IRatingService ratingService)
        {
            _scraperService = scraperService;
            _ratingService = ratingService;
        }

        public async Task<IActionResult> Index(string? search, int? ratingCategoryId, int page = 1)
        {
            const int pageSize = 50;

            var query = await _ratingService.GetRatings();

            if(!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => 
                    x.Competitor.FirstName.Contains(search) || 
                    x.Competitor.LastName.Contains(search));
            }

            if (ratingCategoryId.HasValue) 
            { 
                query = query.Where(x => 
                    x.RatingCategoryId == ratingCategoryId.Value); 
            }

            var totalCount = query.Count(); 
            
            var ratings = query
                .OrderByDescending(x => x.Rating)
                .ThenBy(x => x.Competitor.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CompetitorRatingRowViewModel 
                { 
                    CompetitorId = x.CompetitorId, 
                    FirstName = x.Competitor.FirstName, 
                    LastName = x.Competitor.LastName, 
                    Category = x.RatingCategory.Name, 
                    Rating = (int)x.Rating, 
                    RatingDate = x.RatingDate 
                })
                .ToList(); // Changed from ToListAsync() to ToList()

            var categories = await _ratingService.GetRatingCategories(); 
            var categorySelectList = categories
                .Select(c => new SelectListItem 
                { 
                    Value = c.RatingCategoryId.ToString(), 
                    Text = c.Name 
                })
                .ToList();
            var vm = new RatingsIndexViewModel 
            { 
                Ratings = ratings, 
                Search = search, 
                RatingCategoryId = ratingCategoryId, 
                Categories = categorySelectList, 
                Page = page, 
                PageSize = pageSize, 
                TotalCount = totalCount 
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> RunRatingScrape()
        {
            await _scraperService.RunRatingsScrapeAsync();

            TempData["Success"] = "Rating scrape uitgevoerd.";

            return RedirectToAction(nameof(Index));
        }
    }
}
