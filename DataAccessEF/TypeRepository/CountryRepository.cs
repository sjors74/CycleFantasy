using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class CountryRepository : GenericRepository<Country>, ICountryRepository
    {
        public CountryRepository(DatabaseContext context) : base(context)
        {
        }
        public new Country GetById(int id)
        {
            var country = context.Country.Where(c => c.CountryId.Equals(id)).FirstOrDefault();
            return country;
        }

        public new void Update(Country country)
        {
             context.Country.Update(country);
        }

        public new void Remove(Country country) 
        {
            context.Country.Remove(country);
        }
    }
}
