using Domain.Context;
using Domain.Interfaces;
using Domain.Models;

namespace DataAccessEF.TypeRepository
{
    public class ConfigurationItemRepository : GenericRepository<ConfigurationItem>, IConfigurationItemRepository
    {
        public ConfigurationItemRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
