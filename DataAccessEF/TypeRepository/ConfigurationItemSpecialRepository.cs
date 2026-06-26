using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class ConfigurationItemSpecialRepository : GenericRepository<ConfigurationItemSpecial>, IConfigurationItemSpecialRepository
    {
        public ConfigurationItemSpecialRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> CreateItemSpecial(ConfigurationItemSpecial item)
        {
            var exists = await context.ConfigurationItemSpecials
                .AnyAsync(ci => ci.ConfigurationId == item.ConfigurationId && 
                                ci.Question == item.Question);
            if(exists)  
                return false;

            context.ConfigurationItemSpecials.Add(item);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateItemSpecial(ConfigurationItemSpecial item)
        {
            var exists = await context.ConfigurationItemSpecials
                .AnyAsync(ci => ci.ConfigurationId == item.ConfigurationId && 
                                ci.Question == item.Question &&
                                ci.Id  != item.Id);
            if (exists)
                return false;
            context.ConfigurationItemSpecials.Update(item);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
