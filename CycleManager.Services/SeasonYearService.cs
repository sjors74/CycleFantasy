using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;

namespace CycleManager.Services
{
    public class SeasonYearService : ISeasonYearService
    {
        private readonly ISeasonYearRepository _repository;

        public SeasonYearService(ISeasonYearRepository repository)
        {
            _repository = repository;
        }

        public async Task CreateAsync(SeasonYear year)
        {
            if(await _repository.ExistsAsync(year.Year))
                throw new InvalidOperationException($"Season year {year.Year} already exists.");

            await _repository.AddAsync(year);   
        }

        public async Task DeleteAsync(int id)
        {
            var year = await _repository.GetByIdAsync(id);
            if (year == null)
                return;

            _repository.Delete(year);
        }

        public Task<List<SeasonYear>> GetAllAsync()
        {
            return _repository.GetAllAsync();
        }

        public Task<SeasonYear?> GetByIdAsync(int id)
        {
            return _repository.GetByIdAsync(id);
        }

        public async Task UpdateAsync(SeasonYear year)
        {
            _repository.Update(year);
        }
    }
}
