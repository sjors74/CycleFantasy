using CycleManager.Domain.Models;

namespace CycleManager.Domain.Interfaces
{
    public interface ISeasonYearRepository
    {
        Task<List<SeasonYear>> GetAllAsync();
        Task<SeasonYear?> GetByIdAsync(int id);
        Task AddAsync(SeasonYear year);
        void Update(SeasonYear year);
        void Delete(SeasonYear year);
        Task<bool> ExistsAsync(int year);
        Task SaveChangesAsync();
    }
}
