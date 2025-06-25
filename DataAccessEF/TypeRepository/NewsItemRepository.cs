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

        public  async Task<IEnumerable<NewsItem>> GetAllActiveNewsItems()
        {
            var newsItems = await context.NewsItems
                .Where(ni => ni.IsActive == true)
                .ToListAsync();
            return newsItems;
        }
    }
}
