using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebCycleManager.Models
{
    public class EventViewModel
    {
        public List<EventItemViewModel> Events { get; set; }
        public EventViewModel()
        {
            Events = new List<EventItemViewModel>();
        }
    }
    public class EventItemViewModel : IValidatableObject
    {
        public int Id { get; set; }
        [DisplayName("Evenement")]
        public string Name { get; set; } = string.Empty;
        [DisplayName("Code")]
        public string? Code { get; set; }
        [DisplayName("Jaar")]
        public int Year { get; set; }
        [DisplayName("Startdatum")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [DisplayName("Einddatum")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        public string? Slogan { get; set; }
        [DisplayName("Actief")]
        public bool IsActive { get; set; }
        [DisplayName("Show podium")]
        public bool ShowPodium { get; set; }
        [DisplayName("Landcode")]
        public string? CountryCode { get; set; }
        [DisplayName("Kleurnaam")]
        public string? ColorName { get; set; }
        public int StagesInEvent { get; set; }
        public List<StageViewModel>? Stages { get; set; }
        [DisplayName("Configuratie")]
        public int? ConfigurationId { get; set; }
        public int AantalPosities { get; set; }
        public int SelectedTeamsCount { get; set; }
        public string EventNameDescription
        {
            get
            {
                return $"{Name} (van {StartDate.ToString("dd-MMMM")} tot {EndDate.ToString("dd-MMMM")} {Year})";
            }
        }
        public bool HasStages
        {
            get 
            { 
                return Stages != null; }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate > EndDate)
            {
                yield return new ValidationResult(
                    "Startdatum mag niet later zijn dan de einddatum",
                    new[] { nameof(StartDate), nameof(EndDate) });
            }
        }
    }
}
