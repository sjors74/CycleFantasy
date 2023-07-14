using CycleManager.Services.Interfaces;
using Domain.Models;

namespace WebCycleManager.Helpers
{
    public static class CountrySelectListHelper
    {
        /// <summary>
        /// Get ordered list of all countries to fill selectlist
        /// </summary>
        /// <returns></returns>
        public static async Task<IEnumerable<Country>> GetOrderedCountries(ICountryService countryService)
        {
            var countries = await countryService.GetAll();
            return countries.OrderBy(c => c.CountryNameLong);
        }
    }
}
