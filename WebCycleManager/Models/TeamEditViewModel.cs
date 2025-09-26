using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebCycleManager.Models
{
    public class TeamEditViewModel
    {
        public int TeamId { get; set; }
        public string CurrentTeamName { get; set; }= string.Empty;
        public int? CountryId { get; set; }
        public string PcsName { get; set; } = string.Empty;

        public List<int> AvailableYears { get; set; } = [];
        public List<TeamYearViewModel> TeamYears { get; set; } = [];
        public List<SelectListItem> Countries { get; set; } = [];
    }

    public class TeamYearViewModel
    {
        public int TeamYearId { get; set; }
        public int Year { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
