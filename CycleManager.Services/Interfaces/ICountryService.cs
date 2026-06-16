using Domain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CycleManager.Services.Interfaces
{
    public interface ICountryService
    {
        Task<Country> GetById(int id);
        Task<IEnumerable<Country>> GetAll();
        Task Create(Country entity);
        Task Update(Country entity);
        Task Delete(Country entity);
        Task<IEnumerable<SelectListItem>> GetCountriesAsSelectList(int selectedId = 0);
    }
}
