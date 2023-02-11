using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class Configuration
    {
        [Key]
        public int Id { get; set; }
        public string ConfigurationType { get; set; } = string.Empty;

        public virtual ICollection<ConfigurationItem>? ConfigurationItems { get; set; }

    }
}
