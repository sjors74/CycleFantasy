using Domain.Models;

namespace Domain.Interfaces
{
    public interface INewsItemRepository : IGenericRepository<NewsItem> 
    {
        Task<IEnumerable<NewsItem>> GetAllActiveNewsItems();
        Task<NewsItem?> GetByIdAsync(int id);
        Task CreateAsync(NewsItem item);
        Task UpdateAsync(NewsItem item);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
