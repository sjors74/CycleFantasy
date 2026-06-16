using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class ConfigurationRepository : GenericRepository<Configuration>, IConfigurationRepository
    {
        public ConfigurationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Configuration?> GetConfigurationById(int id)
        {
            return await context.Configurations
                .Include(c => c.ConfigurationItems)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
