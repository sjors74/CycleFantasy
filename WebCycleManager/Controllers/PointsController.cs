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
            var groupedList = results.GroupBy(g => g.CompetitorInEventId).Select(c => new PointsCompetitorInEventViewModel
            {
                FirstName = c.First().CompetitorInEvent.Competitor.FirstName,
                LastName = c.First().CompetitorInEvent.Competitor.LastName,
                EventId = c.First().Stage.EventId,
                CompetitorEventId = c.First().CompetitorInEventId,
                Points = c.Sum(a => a.ConfigurationItem.Score)
            }).OrderByDescending(c => c.Points).ThenBy(c => c.CompetitorName);
            
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

        public async Task<ActionResult> Ranking(int eventId)
        {
                var vm = new List<PointsCompetitorInEventViewModel>();
                var results = await _resultRepository.GetResultsByEventId(eventId);
                var groupedList = results.GroupBy(g => g.CompetitorInEventId).Select(c => new PointsCompetitorInEventViewModel
                {
                    FirstName = c.First().CompetitorInEvent.Competitor.FirstName,
                    LastName = c.First().CompetitorInEvent.Competitor.LastName,
                    EventId = c.First().Stage.EventId,
                    CompetitorEventId = c.First().CompetitorInEventId,
                    Points = c.Sum(a => a.ConfigurationItem.Score)
                }).OrderByDescending(c => c.Points).ThenBy(c => c.CompetitorName);

            //var gameCompetitors = _resultRepository.GetGameCompetitorsPicks(eventId);
            //foreach(var gameCompetitor in gameCompetitors)
            //{
            //    var gcId = gameCompetitor.GameCompetitorEventId;
            //    var competitors = _resultRepository.GetCompetitors(eventId, gcId);
            //}
            return View("test");
        }
    }
}
