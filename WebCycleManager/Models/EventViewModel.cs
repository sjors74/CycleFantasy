using Domain.Models;
using Microsoft.AspNetCore.Mvc;
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
    public class EventItemViewModel
    {
        public int Id { get; set; }
        [DisplayName("Evenement")]
        public string Name { get; set; } = string.Empty;
        [DisplayName("Jaar")]
        public int Year { get; set; }
        [DisplayName("Startdatum")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [DisplayName("Einddatum")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        [DisplayName("Actief")]
        public bool IsActive { get; set; }
        public int StagesInEvent { get; set; }
        public List<StageViewModel>? Stages { get; set; }
        [DisplayName("Configuratie")]
        public int? ConfigurationId { get; set; }
        public int AantalPosities { get; set; }
        public string EventNameDescription
        {
            get
            {
                return $"{Name} (van {StartDate.ToString("dd-MMMM")} tot {EndDate.ToString("dd-MMMM")} {Year})";
            }
        }
    }
}
