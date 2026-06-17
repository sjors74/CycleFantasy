using CycleManager.Services.Interfaces;
using Domain.Interfaces;
using Domain.Models;

namespace CycleManager.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IConfigurationItemRepository _configurationItemRepository;
        private readonly IConfigurationItemSpecialRepository _configurationItemSpecialRepository;

        public ConfigurationService(IConfigurationRepository configurationRepository, 
            IConfigurationItemRepository configurationItemRepository,
            IConfigurationItemSpecialRepository configurationItemSpecialRepository)
        {
            _configurationRepository = configurationRepository;
            _configurationItemRepository = configurationItemRepository;
            _configurationItemSpecialRepository = configurationItemSpecialRepository;
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
        public async Task<bool> CreateItem(ConfigurationItem entity)
        {
            var success = await _configurationItemRepository.CreateItem(entity);
            return success;
        }

        public Task<bool> CreateItemSpecial(ConfigurationItemSpecial entity)
        {
            var success = _configurationItemSpecialRepository.CreateItemSpecial(entity);
            return success;
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

        public async Task DeleteItemSpecial(ConfigurationItemSpecial entity)
        {
            _configurationItemSpecialRepository.Remove(entity);
            await _configurationItemSpecialRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Get all configuration-items
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ConfigurationItem>> GetAllConfigurationItems()
        {
            return await _configurationItemRepository.GetAll();
        }

        public async Task<IEnumerable<ConfigurationItemSpecial>> GetAllConfigurationItemSpecials()
        {
            return await _configurationItemSpecialRepository.GetAll();
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
            return _configurationRepository.GetConfigurationById(id);
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

        public Task<ConfigurationItemSpecial> GetConfigurationItemSpecialById(int id)
        {
            return _configurationItemSpecialRepository.GetById(id);
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
        public async Task<bool> UpdateItem(ConfigurationItem entity)
        {
            var success = await _configurationItemRepository.UpdateItem(entity);
            return success;
        }

        public async Task<bool> UpdateItemSpecial(ConfigurationItemSpecial entity)
        {
            var success = await _configurationItemSpecialRepository.UpdateItemSpecial(entity);
            return success;
        }
    }
}