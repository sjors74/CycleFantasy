using Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface INewsService
    {
        Task<IEnumerable<NewsItem>> GetAllActiveNewsItems();
        Task<NewsItem?> GetByIdAsync(int id);
        Task CreateAsync(NewsItem item);
        Task UpdateAsync(NewsItem item);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}