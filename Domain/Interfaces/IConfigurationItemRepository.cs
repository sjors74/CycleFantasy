using Domain.Models;

namespace Domain.Interfaces
{
    public interface IConfigurationItemRepository : IGenericRepository<ConfigurationItem>
    {
        Task<bool> CreateItem(ConfigurationItem configurationItem);
        Task<bool> UpdateItem(ConfigurationItem configurationItem);
    }
}
