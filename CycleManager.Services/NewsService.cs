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

        public async Task CreateAsync(NewsItem item)
        {
            await _repository.CreateAsync(item);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _repository.ExistsAsync(id);
        }

        public async Task<IEnumerable<NewsItem>> GetAllActiveNewsItems()
        {
            return await _repository.GetAllActiveNewsItems();
        }

        public async Task<NewsItem?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task UpdateAsync(NewsItem item)
        {
            await _repository.UpdateAsync(item);
        }
    }
}
