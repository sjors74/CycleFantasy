using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class GameCompetitor
    {
        [Key]
        public int Id { get; set; }
        [DisplayName("Voornaam")]
        public string FirstName { get; set; } = string.Empty;
        [DisplayName("Achternaam")]
        public string LastName { get; set; } = string.Empty;

        public GameCompetitor() { }

    }
}
