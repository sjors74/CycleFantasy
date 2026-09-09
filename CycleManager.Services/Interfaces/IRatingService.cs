using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface IRatingService
    {
        Task<IEnumerable<CompetitorRating>> GetRatings();

        Task<IEnumerable<RatingCategory>> GetRatingCategories();

        Task<IEnumerable<DeelnemerRatingDto>> GetGameCompetitorRatings(int eventId);

        Task<IEnumerable<CompetitorRating>> GetRatingsByCompetitorIds(IEnumerable<int> competitorIds);
    }
}