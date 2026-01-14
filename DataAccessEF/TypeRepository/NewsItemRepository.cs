using Domain.Interfaces;
using Domain.Models;
using Domain.Context;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class NewsItemRepository : GenericRepository<NewsItem>, INewsItemRepository
    {
        public NewsItemRepository(ApplicationDbContext context) : base(context) 
        { 
        }

        public async Task CreateAsync(NewsItem item)
        {
            context.NewsItems.Add(item);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var item = await context.NewsItems.FindAsync(id);
            if (item != null)
            {
                context.NewsItems.Remove(item);
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await context.NewsItems.AnyAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<NewsItem>> GetAllActiveNewsItems()
        {
            var newsItems = await context.NewsItems
                .Where(ni => ni.IsActive == true)
                .ToListAsync();
            return newsItems;
        }

        public async Task<NewsItem?> GetByIdAsync(int id)
        {
            return await context.NewsItems.FindAsync(id);
        }

        public async Task UpdateAsync(NewsItem item)
        {
            var existingItem = await context.NewsItems.FindAsync(item.Id);
            if (existingItem == null) throw new InvalidOperationException("News item not found");

            existingItem.Title = item.Title;
            existingItem.Message = item.Message;
            existingItem.DatePosted = item.DatePosted;
            existingItem.IsActive = item.IsActive;

            await context.SaveChangesAsync();
        }
    }
}
