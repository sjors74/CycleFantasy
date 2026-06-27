using Domain.Models;

namespace Domain.Interfaces
{
    public interface IConfigurationItemSpecialRepository : IGenericRepository<ConfigurationItemSpecial>
    {
        Task<bool> CreateItemSpecial(ConfigurationItemSpecial configurationItemSpecial);
        Task<bool> UpdateItemSpecial(ConfigurationItemSpecial configurationItemSpecial);
    }
}
