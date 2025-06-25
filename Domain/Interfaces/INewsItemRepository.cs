using Domain.Models;

namespace Domain.Interfaces
{
    public interface INewsItemRepository : IGenericRepository<NewsItem> 
    {
        Task<IEnumerable<NewsItem>> GetAllActiveNewsItems();
    }
}
