using CycleManager.Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface IRatingService
    {
        Task<IEnumerable<CompetitorRating>> GetRatings();

        Task<IEnumerable<RatingCategory>> GetRatingCategories();
    }
}