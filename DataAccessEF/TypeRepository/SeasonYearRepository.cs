using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using Domain.Context;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class SeasonYearRepository : GenericRepository<SeasonYear>, ISeasonYearRepository
    {
        public SeasonYearRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task AddAsync(SeasonYear year)
        {
            await context.SeasonYears.AddAsync(year);
        }

        public void Delete(SeasonYear year)
        {
            context.SeasonYears.Remove(year);
        }

        public async Task<bool> ExistsAsync(int year)
        {
            return await context.SeasonYears
           .AnyAsync(x => x.Year == year);
        }

        public async Task<List<SeasonYear>> GetAllAsync()
        {
            return await context.SeasonYears
            .OrderByDescending(x => x.Year)
            .ToListAsync();
        }

        public async Task<SeasonYear?> GetByIdAsync(int id)
        {
            return await context.SeasonYears
           .FirstOrDefaultAsync(x => x.SeasonYearId == id);
        }

        public new void Update(SeasonYear year)
        {
            context.SeasonYears.Update(year);
        }
    }
}
