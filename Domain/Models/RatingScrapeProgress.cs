using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CycleManager.Domain.Models
{
    public class RatingScrapeProgress
    {
        [Key]
        public int RatingScrapeProgressId { get; set; }

        [Required]
        public int RatingCategoryId { get; set; }

        [ForeignKey(nameof(RatingCategoryId))]
        public RatingCategory RatingCategory { get; set; } = null!;

        /// <summary>
        /// Laatst succesvol gescrapete pagina.
        /// Begint op 0, zodat de eerste scrape pagina 1 wordt.
        /// </summary>
        public int LastPage { get; set; } = 0;

        /// <summary>
        /// Wanneer deze categorie voor het laatst succesvol is verwerkt.
        /// </summary>
        public DateTime? LastScrapeDate { get; set; }

        /// <summary>
        /// Aantal opeenvolgende fouten.
        /// </summary>
        public int ErrorCount { get; set; } = 0;

        /// <summary>
        /// Laatste foutmelding (optioneel).
        /// </summary>
        [MaxLength(500)]
        public string? LastError { get; set; }
    }
}
