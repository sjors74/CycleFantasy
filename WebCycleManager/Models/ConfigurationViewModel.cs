using CycleManager.Domain.Enums;
using System.ComponentModel;

namespace WebCycleManager.Models
{
    public class ConfigurationViewModel
    {
        public int Id { get; set; }
        [DisplayName("Configuratie naam")]
        public string ConfigurationName { get; set; } = string.Empty;
        [DisplayName("Configuratie items")]
        public List<ConfigurationItemViewModel> ConfigurationItems { get; set; } = new List<ConfigurationItemViewModel>();
        [DisplayName("Speciale configuratie items")]
        public List<ConfigurationItemsSpecialViewModel> ConfigurationItemSpecials { get; set; } = new();
    }

    public class ConfigurationItemViewModel
    {
        public int Id { get; set; }
        [DisplayName("Positie")]
        public int Position { get; set; }
        public int Score { get; set; }
        [DisplayName("Configuratie")]
        public int ConfigurationId { get; set; }
    }

    public class ConfigurationItemsSpecialViewModel
    {
        public int Id { get; set; }
        [DisplayName("Vraag")]
        public QuestionType Question { get; set; }
        public int Score { get; set; }
        [DisplayName("Configuratie")]
        public int ConfigurationId { get; set; }
    }
}
