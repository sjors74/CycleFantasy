using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class CountryRepository : GenericRepository<Country>, ICountryRepository
    {
        public CountryRepository(ApplicationDbContext context) : base(context)
        {
        }
        public new Country GetById(int id)
        {
            var country = context.Countries.Where(c => c.CountryId.Equals(id)).FirstOrDefault();
            return country;
        }

        public new void Update(Country country)
        {
             context.Countries.Update(country);
        }

        public new void Remove(Country country) 
        {
            context.Countries.Remove(country);
        }
    }
}
