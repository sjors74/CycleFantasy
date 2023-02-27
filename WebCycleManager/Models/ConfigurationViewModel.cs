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
}
