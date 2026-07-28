using CycleManager.Domain.Models;

namespace CycleManager.Domain.Interfaces
{
    public interface IRatingRepository
    {
        Task<IEnumerable<CompetitorRating>> GetRatings();

        Task<IEnumerable<RatingCategory>> GetRatingCategories();
    }
}
