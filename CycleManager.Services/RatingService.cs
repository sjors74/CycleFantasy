using CycleManager.Domain.Dto;
using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;

namespace CycleManager.Services
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _ratingRepository;

        public RatingService(IRatingRepository ratingRepository)
        {
            _ratingRepository = ratingRepository;
        }

        public Task<IEnumerable<RatingCategory>> GetRatingCategories()
        {
            return _ratingRepository.GetRatingCategories();
        }

        public Task<IEnumerable<CompetitorRating>> GetRatings()
        {
            return _ratingRepository.GetRatings();
        }

        public async Task<IEnumerable<DeelnemerRatingDto>> GetGameCompetitorRatings(int eventId)
        {
            return await _ratingRepository.GetGameCompetitorRatings(eventId);
        }

        public async Task<IEnumerable<CompetitorRating>> GetRatingsByCompetitorIds(IEnumerable<int> competitorIds)
        {
            return await _ratingRepository.GetRatingsByCompetitorIds(competitorIds);
        }
    }
}
