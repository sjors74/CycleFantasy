using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class PointsController : Controller
    {
        private readonly IResultsRepository _resultRepository;
        
        public PointsController(IResultsRepository resultRepository)
        {
            _resultRepository = resultRepository;
        }
        public async Task<IActionResult> Index(int eventId)
        {
            var vm = new List<PointsCompetitorInEventViewModel>();
            var results = await _resultRepository.GetResultsByEventId(eventId);
            var groupedList = results
                .Where(r => r.Stage.EventId == eventId)
                .Select(r => new
                {
                    CompetitorEventId = r.CompetitorInEventId,
                    Score = r.ConfigurationItem == null ? 0 : r.ConfigurationItem.Score,
                    FirstName = r.CompetitorInEvent.CompetitorInTeam.Competitor.FirstName,
                    LastName = r.CompetitorInEvent.CompetitorInTeam.Competitor.LastName,
                    EventId = r.Stage.EventId
                })
                .GroupBy(x => new {
                    x.CompetitorEventId,
                    x.FirstName,
                    x.LastName,
                    x.EventId
                })
                .Select(g => new PointsCompetitorInEventViewModel
                {
                    FirstName = g.Key.FirstName,
                    LastName = g.Key.LastName,
                    EventId = g.Key.EventId,
                    CompetitorEventId = g.Key.CompetitorEventId,
                    Points = g.Sum(x => x.Score)
                })
                .OrderByDescending(x => x.Points)
                .ThenBy(x => x.LastName).ThenBy(x => x.FirstName)
                .ToList();

            List<IGrouping<int, PointsCompetitorInEventViewModel>> orderedCompetitors = 
                groupedList.GroupBy(g => g.Points).OrderByDescending(g => g.Key).ToList();

            for (int i = 0; i < orderedCompetitors.Count(); i++)
            {
                IGrouping<int, PointsCompetitorInEventViewModel> grouping = orderedCompetitors[i];
                foreach (var element in grouping)
                {
                    element.Ranking = i + 1;
                }
            }
            vm = orderedCompetitors.SelectMany(c => c).ToList();
            return View(vm);
        }
    }
}
