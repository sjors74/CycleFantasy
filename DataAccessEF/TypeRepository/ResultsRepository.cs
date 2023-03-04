using Domain.Context;
using Domain.Dto;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class ResultsRepository : GenericRepository<Result>, IResultsRepository
    {
        public ResultsRepository(DatabaseContext context) : base(context)
        {

        }

        public async Task<IEnumerable<Result>> GetResultsByEventId(int eventId)
        {
            //var results = new List<ResultDto>();
            var results = await context.Results
                .Include(c => c.CompetitorInEvent)
                .Include(s => s.Stage)
                .Where(r => r.Stage.EventId == eventId)
                .OrderBy(r => r.ConfigurationItem.Position).ToListAsync();

            //foreach(var resultItem in resultsDb)
            //{
            //    results.Add(
            //        new ResultDto
            //        {
            //            CompetitorName = resultItem.CompetitorInEvent.Competitor.CompetitorName,
            //            Points = resultItem.ConfigurationItem.Score,
            //            Position = resultItem.ConfigurationItem.Position,
            //            StageNumber = resultItem.Stage.StageName,
            //            CompetitorInEventId = resultItem.CompetitorInEventId
            //        });
            //}

            return results;
        }

        public async Task<int> GetResultsByStageId(int stageId)
        {
            return 0;
        }
    }
}
