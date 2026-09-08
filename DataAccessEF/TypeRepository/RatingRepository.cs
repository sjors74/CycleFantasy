using CycleManager.Domain.Dto;
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

        public async Task<IEnumerable<CompetitorRating>> GetRatingsByCompetitorId(int competitorId)
        {
            var ratings = await context.CompetitorRatings
                .Include(x => x.Competitor)
                .Include(x => x.RatingCategory)
                .Where(x => x.CompetitorId == competitorId)
                .AsQueryable()
                .ToListAsync();

            return ratings;
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

        public async Task<IEnumerable<DeelnemerRatingDto>> GetGameCompetitorRatings(int eventId)
        {
            var query =
                from pick in context.GameCompetitorEventPicks

                join ce in context.CompetitorsInEvent
                    on pick.CompetitorsInEventId equals ce.Id

                join cit in context.CompetitorInTeams
                    on ce.CompetitorInTeamId equals cit.Id

                join rating in context.CompetitorRatings
                    on cit.CompetitorId equals rating.CompetitorId

                join category in context.RatingCategories
                    on rating.RatingCategoryId equals category.RatingCategoryId

                where pick.GameCompetitorEvent.EventId == eventId

                where category.IsActive
                // Alleen de laatste rating van deze renner voor deze categorie
                where !context.CompetitorRatings.Any(r2 =>
                    r2.CompetitorId == rating.CompetitorId &&
                    r2.RatingCategoryId == rating.RatingCategoryId &&
                    (
                        r2.RatingDate > rating.RatingDate ||
                        (
                            r2.RatingDate == rating.RatingDate &&
                            r2.CompetitorRatingId > rating.CompetitorRatingId
                        )
                    ))

                group rating by new
                {
                    pick.GameCompetitorEventId,
                    rating.RatingCategoryId,
                    category.Name,
                    category.Color,
                    category.DisplayOrder
                }
                into g

                select new DeelnemerRatingDto
                {
                    GameCompetitorEventId = g.Key.GameCompetitorEventId,
                    RatingCategoryId = g.Key.RatingCategoryId,
                    RatingCategoryName = g.Key.Name,
                    Color = g.Key.Color,
                    DisplayOrder = g.Key.DisplayOrder,
                    Rating = g.Average(x => x.Rating)
                };

            return await query
                .OrderBy(x => x.GameCompetitorEventId)
                .ThenBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<CompetitorRating>> GetRatingsByCompetitorIds(IEnumerable<int> competitorIds)
        {
            return await context.CompetitorRatings
                .Include(x => x.RatingCategory)
                .Where(x => competitorIds.Contains(x.CompetitorId))
                .ToListAsync();
        }
    }
}
