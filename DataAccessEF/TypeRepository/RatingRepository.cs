using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using Domain.Context;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class RatingRepository : GenericRepository<CompetitorRating>, IRatingRepository
    {
        public RatingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<RatingCategory>> GetRatingCategories()
        {
            var ratingCategories = await context.RatingCategories
                .OrderBy(c => c.Name)
                .ToListAsync();
            return ratingCategories;
        }

        public async Task<IEnumerable<CompetitorRating>> GetRatings()
        {
            var ratings = await context.CompetitorRatings
                .Include(x => x.Competitor)
                .Include(x => x.RatingCategory)
                .AsQueryable()
                .ToListAsync();

            return ratings;
        }
    }
}
