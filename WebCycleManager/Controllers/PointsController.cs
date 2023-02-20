using Domain.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class PointsController : Controller
    {
        private readonly DatabaseContext _context;
        
        public PointsController(DatabaseContext context)
        {
            _context = context;
        }
        public IActionResult Index(int eventId)
        {
            var vm = new List<PointsCompetitorInEventViewModel>();
            var competitors = _context.Results.Where(r => r.Stage.EventId.Equals(eventId)).ToList();
            var groupedList = competitors.GroupBy(g => g.CompetitorInEventId).Select(c => new PointsCompetitorInEventViewModel
            {
                FirstName = c.First().CompetitorInEvent.Competitor.FirstName,
                LastName = c.First().CompetitorInEvent.Competitor.LastName,
                EventId = c.First().Stage.EventId,
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
    }
}
