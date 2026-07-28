using CycleManager.Domain.Enums;
using CycleManager.Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface ICyclingFlashScraper
    {
        Task<List<ScrapeCompetitorRating>> ScrapePageResultAsync(
            string category,
            int pageNumber,
            DateTime ratingDate);
    }
}
