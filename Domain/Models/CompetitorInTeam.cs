using Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace CycleManager.Domain.Models
{
    public class CompetitorInTeam
    {
        [Key]
        public int Id { get; set; }
        public int CompetitorId { get; set; }
        public Competitor Competitor { get; set; } = null!;
        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;
        public int Year { get; set; }
        public bool IsNationalChampion { get; set; }
    }
}
