using System.ComponentModel;

namespace WebCycleManager.Models
{
    public class CountryViewModel
    {
        public int Id { get; set; }
        [DisplayName("Land")]
        public string Name { get; set; } = string.Empty;
        [DisplayName("Afkorting")]
        public string ShortName { get; set; } = string.Empty;
        [DisplayName("Aantal renners")]
        public int CompetitorsCount { get; set; }
    }
}
