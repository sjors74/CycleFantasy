using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Models
{
    public class ConfigurationItem
    {
        [Key]
        public int Id { get; set; }
        public int Position { get; set; }
        public int Score { get; set; }
        public int ConfigurationId { get; set; }
        [JsonIgnore]
        public virtual Configuration? Configuration { get; set; }
    }
}
