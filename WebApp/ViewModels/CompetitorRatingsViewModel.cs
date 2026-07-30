using CycleManager.Domain.Dto;

namespace WebApp.ViewModels
{
    public class CompetitorRatingsViewModel
    {
        public List<CompetitorRatingDto> Ratings { get; set; } = new List<CompetitorRatingDto>();
        public int MaxRating { get; set; }
    }
}
