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
        Task CreateItem(ConfigurationItem entity);
        Task UpdateItem(ConfigurationItem entity);
        Task DeleteItem(ConfigurationItem entity);


    }
}
