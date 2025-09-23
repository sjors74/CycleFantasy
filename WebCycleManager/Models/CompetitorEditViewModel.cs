using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebCycleManager.Models.ViewModel
{
    public class CompetitorEditInputModel
    {
        // Basis Competitor gegevens
        public int CompetitorId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PcsName { get; set; }
        public int CountryId { get; set; }

        // Beschikbare teams (dropdown)
        public int SelectedTeamId { get; set; }

        // CompetitorInTeam gegevens
        public int SelectedYear { get; set; }

        // Eventueel extra properties uit CompetitorInTeam
        public bool IsNationalChampion { get; set; }
    }

    public class CompetitorEditViewModel : CompetitorEditInputModel
    {
        public IEnumerable<SelectListItem> Teams { get; set; }
        public IEnumerable<SelectListItem> AvailableYears { get; set; }
        public IEnumerable<SelectListItem> Countries { get; set; }

    }
}
