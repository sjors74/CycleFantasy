using CycleManager.Domain.Enums;
using CycleManager.Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface ICyclingFlashScraper
    {
        /// <summary>
        /// Scrape Cyclingflash.com for ratings, based on a category (code)
        /// a pagenumber and a date.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="pageNumber"></param>
        /// <param name="ratingDate"></param>
        /// <returns></returns>
        Task<List<ScrapeCompetitorRating>> ScrapePageResultAsync(
            string category,
            int pageNumber,
            DateTime ratingDate);
        /// <summary>
        /// Scrape Cyclingflash.com for ratings based on a profile-name ("renner")
        /// and a date.
        /// </summary>
        /// <param name="profileUrl"></param>
        /// <param name="ratingDate"></param>
        /// <returns></returns>
        Task<List<ScrapeCompetitorRating>> ScrapeCompetitorRatingsAsync(
            string profileUrl,
            DateTime ratingDate);
    }
}
