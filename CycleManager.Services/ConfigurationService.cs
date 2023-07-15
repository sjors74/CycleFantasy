using CycleManager.Services.Interfaces;
using Domain.Interfaces;
using Domain.Models;

namespace CycleManager.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IConfigurationItemRepository _configurationItemRepository;

        public ConfigurationService(IConfigurationRepository configurationRepository, IConfigurationItemRepository configurationItemRepository)
        {
            _configurationRepository = configurationRepository;
            _configurationItemRepository = configurationItemRepository;
        }

        /// <summary>
        /// Create a new configuration and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Create(Configuration entity)
        {
            _configurationRepository.Add(entity);
            await _configurationRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Create a new configuration-item and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task CreateItem(ConfigurationItem entity)
        {
            _configurationItemRepository.Add(entity);
            await _configurationItemRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Delete a configuration and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Delete(Configuration entity)
        {
            _configurationRepository.Remove(entity);
            await _configurationRepository.SaveChangesAsync();

        }

        /// <summary>
        /// Delete a configuration-item and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task DeleteItem(ConfigurationItem entity)
        {
            _configurationItemRepository.Remove(entity);
            await _configurationItemRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Get all configuration-items
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ConfigurationItem>> GetAllConfigurationItems()
        {
            return await _configurationItemRepository.GetAll();
        }

        /// <summary>
        /// Get all configurations
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<Configuration>> GetAllConfigurations()
        {
            return  _configurationRepository.GetAll();
        }

        /// <summary>
        /// Get a configuration by it's id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<Configuration> GetConfigurationById(int id)
        {
            return _configurationRepository.GetById(id);
        }

        /// <summary>
        /// Get a configuration-item by it's id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<ConfigurationItem> GetConfigurationItemById(int id)
        {
            return _configurationItemRepository.GetById(id);
        }

        /// <summary>
        /// Update a configuration and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Update(Configuration entity)
        {
            _configurationRepository.Update(entity);
            await _configurationRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Update a configuration-item and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task UpdateItem(ConfigurationItem entity)
        {
            _configurationItemRepository.Update(entity);
            await _configurationItemRepository.SaveChangesAsync();
        }
    }
}