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
                throw new InvalidOperationException($"Jaar {year.Year} bestaat al.");

            await _repository.AddAsync(year);   

            await _repository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var year = await _repository.GetByIdAsync(id);
            if (year == null)
                return;

            //don't delete, just set to inactive
            year.Active = false;
            _repository.Update(year);
            await _repository.SaveChangesAsync();
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
            await _repository.SaveChangesAsync();
        }
    }
}
