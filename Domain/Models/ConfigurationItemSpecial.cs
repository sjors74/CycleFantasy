using CycleManager.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Models
{
    public class ConfigurationItemSpecial
    {
        [Key]
        public int Id { get; set; }
        public QuestionType Question { get; set; }
        public int Score { get; set; }
        public int ConfigurationId { get; set; }
        [JsonIgnore]
        public virtual Configuration? Configuration { get; set; }
    }
}
