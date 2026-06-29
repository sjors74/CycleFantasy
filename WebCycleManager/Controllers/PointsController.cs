using CycleManager.Services.Interfaces;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class PointsController : Controller
    {
        private readonly IResultsRepository _resultRepository;
        private readonly IResultService _resultService;
        
        public PointsController(IResultsRepository resultRepository, IResultService resultService)
        {
            _resultRepository = resultRepository;
            _resultService = resultService;
        }
        public async Task<IActionResult> Index(int eventId)
        {
            var vm = await _resultService.GetResultsByEventId(eventId);

            return View(vm);
        }
    }
}
