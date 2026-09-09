using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;

namespace CycleManager.Domain.Interfaces
{
    public interface IRatingRepository
    {
        Task<IEnumerable<CompetitorRating>> GetRatings();

        Task<IEnumerable<RatingCategory>> GetRatingCategories();

        Task<IEnumerable<CompetitorRating>> GetRatingsByCompetitorId(int competitorId);

        Task<IEnumerable<DeelnemerRatingDto>> GetGameCompetitorRatings(int eventId);

        Task<IEnumerable<CompetitorRating>> GetRatingsByCompetitorIds(IEnumerable<int> competitorIds);
    }
}
