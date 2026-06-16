using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class ConfigurationItemRepository : GenericRepository<ConfigurationItem>, IConfigurationItemRepository
    {
        public ConfigurationItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> CreateItem(ConfigurationItem item)
        {
            var exists = await context.ConfigurationItems
                .AnyAsync(ci => ci.ConfigurationId == item.ConfigurationId && 
                                ci.Position == item.Position);
            if(exists)  
                return false;

            context.ConfigurationItems.Add(item);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateItem(ConfigurationItem item)
        {
            var exists = await context.ConfigurationItems
                .AnyAsync(ci => ci.ConfigurationId == item.ConfigurationId && 
                                ci.Position == item.Position &&
                                ci.Id  != item.Id);
            if (exists)
                return false;
            context.ConfigurationItems.Update(item);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
