using Domain.Context;
using Domain.Interfaces;
using Domain.Models;

namespace DataAccessEF.TypeRepository
{
    public class ConfigurationItemRepository : GenericRepository<ConfigurationItem>, IConfigurationItemRepository
    {
        public ConfigurationItemRepository(DatabaseContext context) : base(context)
        {
        }
    }
}
