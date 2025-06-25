using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class NewsItem
    {
        [Key]
        public int Id { get; set; }
        [DisplayName("Onderwerp")]
        public required string Title { get; set; }
        [DisplayName("Bericht")]
        public required string Message { get; set; }
        [DisplayName("Geplaatst op")]
        public DateTime DatePosted { get; set; }
        [DisplayName("Is actief")]
        public Boolean IsActive { get; set; } = false;

    }
}
