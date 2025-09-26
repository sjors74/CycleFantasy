using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebCycleManager.Models.ViewModel
{
    public class CompetitorEditInputModel
    {
        public int CompetitorId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PcsName { get; set; }
        public string? ScraperName { get; set; }
        public int CountryId { get; set; }

        public List<CompetitorInTeamInputModel> CompetitorInTeams { get; set; } = new();
    }
    public class CompetitorInTeamInputModel
    {
        public int CompetitorInTeamId { get; set; }
        public int Year { get; set; }
        public bool IsNationalChampion { get; set; }
        public int TeamId { get; set; }
    }

    public class CompetitorEditViewModel
    {
        public int CompetitorId { get; set; }
        [DisplayName("Voornaam")]
        public string FirstName { get; set; } = string.Empty;
        [DisplayName("Achternaam")]
        public string LastName { get; set; } = string.Empty;
        public string? PcsName { get; set; }
        public string? ScraperName { get; set; }
        [DisplayName("Land")]
        public int CountryId { get; set; }
        public int SelectedTeamId { get; set; }
        public int SelectedYear { get; set; }

        public IEnumerable<SelectListItem> Countries { get; set; }
        public IEnumerable<SelectListItem> Teams { get; set; }
        public IEnumerable<SelectListItem> AvailableYears { get; set; }

        public List<CompetitorInTeamEditModel> CompetitorInTeams { get; set; } = new();
    }

    public class CompetitorInTeamEditModel
    {
        public int CompetitorInTeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public int Year { get; set; }
        public bool IsNationalChampion { get; set; }
        public int TeamId { get; set; }
    }
}
