using Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface INewsService
    {
        Task<IEnumerable<NewsItem>> GetAllActiveNewsItems();
    }
}
