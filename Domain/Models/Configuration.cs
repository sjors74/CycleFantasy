using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Models
{
    public class Configuration
    {
        [Key]
        public int Id { get; set; }
        public string ConfigurationType { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual ICollection<ConfigurationItem>? ConfigurationItems { get; set; }

    }
}
