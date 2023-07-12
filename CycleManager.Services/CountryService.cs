using CycleManager.Services.Interfaces;
using Domain.Interfaces;
using Domain.Models;

namespace CycleManager.Services
{
    public class CountryService : ICountryService
    {
        private readonly ICountryRepository _countryRepository;
        public CountryService(ICountryRepository countryRepository)
        {
            _countryRepository = countryRepository;
        }

        public async Task Create(Country entity)
        {
            _countryRepository.Add(entity);
            await _countryRepository.SaveChangesAsync();
        }

        public async Task Delete(Country entity)
        {
            _countryRepository.Remove(entity);
            await _countryRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<Country>> GetAll()
        {
            return await _countryRepository.GetAll();
        }

        public async Task<Country> GetById(int id)
        {
            return await _countryRepository.GetById(id);
        }

        public async Task Update(Country entity)
        {
            _countryRepository.Update(entity);
            await _countryRepository.SaveChangesAsync();
        }

    }
}