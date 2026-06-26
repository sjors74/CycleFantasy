using Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface IConfigurationService
    {
        Task<IEnumerable<Configuration>> GetAllConfigurations();
        Task<Configuration> GetConfigurationById(int id);
        Task Create(Configuration entity);
        Task Update(Configuration entity);
        Task Delete(Configuration entity);

        Task<IEnumerable<ConfigurationItem>> GetAllConfigurationItems();
        Task<ConfigurationItem> GetConfigurationItemById(int id);
        Task<bool> CreateItem(ConfigurationItem entity);
        Task<bool> UpdateItem(ConfigurationItem entity);
        Task DeleteItem(ConfigurationItem entity);

        Task<IEnumerable<ConfigurationItemSpecial>> GetAllConfigurationItemSpecials();
        Task<ConfigurationItemSpecial> GetConfigurationItemSpecialById(int id);
        Task<bool> CreateItemSpecial(ConfigurationItemSpecial entity);
        Task<bool> UpdateItemSpecial(ConfigurationItemSpecial entity);
        Task DeleteItemSpecial(ConfigurationItemSpecial entity);

    }
}
