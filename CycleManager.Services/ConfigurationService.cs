using CycleManager.Services.Interfaces;
using Domain.Interfaces;
using Domain.Models;

namespace CycleManager.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationRepository _configurationRepository;
        public ConfigurationService(IConfigurationRepository configurationRepository)
        {
            _configurationRepository = configurationRepository;
        }

        public Task<IEnumerable<Configuration>> GetAllConfigurations()
        {
            return  _configurationRepository.GetAll();
        }
    }
}
