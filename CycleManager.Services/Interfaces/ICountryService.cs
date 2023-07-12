using Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface ICountryService
    {
        Task<Country> GetById(int id);
        Task<IEnumerable<Country>> GetAll();
        Task Create(Country entity);
        Task Update(Country entity);
        Task Delete(Country entity);
    }
}
