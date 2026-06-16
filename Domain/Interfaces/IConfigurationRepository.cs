using Domain.Models;

namespace Domain.Interfaces
{
    public interface IConfigurationRepository : IGenericRepository<Configuration>
    {
        Task<Configuration?> GetConfigurationById(int id);
    }
}
