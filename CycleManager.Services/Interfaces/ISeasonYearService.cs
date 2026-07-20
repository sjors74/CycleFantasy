using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface ISeasonYearService
    {
        Task<List<SeasonYearDto>> GetAllAsync();
        Task<SeasonYear?> GetByIdAsync(int id);
        Task CreateAsync(SeasonYear year);
        Task UpdateAsync(SeasonYear year);
        Task DeleteAsync(int id);
    }
}
