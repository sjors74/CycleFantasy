using DataAccessEF.Extensions;
using Domain.Dto;

namespace WebCycleManager.Models
{
    public class CompetitorIndexViewModel
    {
        public PaginatedList<CompetitorDto> Competitors { get; set; } = null!;

        public List<SeasonYearViewModel> AvailableYears { get; set; } = [];

        public int SelectedSeasonYearId { get; set; }

        public string? CurrentFilter { get; set; }
    }
}
