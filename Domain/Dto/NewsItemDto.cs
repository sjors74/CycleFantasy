using System.ComponentModel;

namespace CycleManager.Domain.Dto
{
    public class NewsItemDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        [DisplayName("Bericht")]
        public required string Message { get; set; }
        [DisplayName("Geplaatst op")]
        public DateTime DatePosted { get; set; }
        [DisplayName("Is actief")]
        public Boolean IsActive { get; set; } = false;
    }
}
