using Domain.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebCycleManager.Models
{
    public class EventViewModel
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
        public int StagesInEvent { get; set; }
        public IEnumerable<Stage>? Stages { get; set; }
    }
}
