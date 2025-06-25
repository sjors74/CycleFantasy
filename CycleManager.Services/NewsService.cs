using Domain.Interfaces;
using Domain.Models;
using CycleManager.Services.Interfaces;

namespace CycleManager.Services
{
    public class NewsService : INewsService
    {
        private readonly INewsItemRepository _repository;
        public NewsService(INewsItemRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<NewsItem>> GetAllActiveNewsItems()
        {
            return _repository.GetAllActiveNewsItems();
        }
    }
}
