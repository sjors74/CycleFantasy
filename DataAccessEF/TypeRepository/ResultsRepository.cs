using Domain.Context;
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
            var results = await context.Results
                                .Include(s => s.Stage)
                                .Where(s => s.StageId == stageId)
                                .ToListAsync();
            return results.Count;
        }


        //public List<GameCompetitorEventPick> GetGameCompetitorsPicks(int eventId)
        //{
        //    var results = context.GameCompetitorEventPicks.Where(c => c.GameCompetitorEventId == eventId).ToList();
        //    return results; 
        //}

        //public List<GameCompetitorEventPick> GetCompetitors(int eventId, int gameCompetitorId)
        //{
        //    var results = context.GameCompetitorEventPicks.Include(c => c.CompetitorsInEvent)
        //        .Where(c => c.GameCompetitorEventId == eventId).ToList();
        //    //&& c gameCompetitorId).ToList();
        //    return results;
        //}

    }
}
