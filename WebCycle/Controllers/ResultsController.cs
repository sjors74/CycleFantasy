using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebCycle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly IResultService _resultService;
        public ResultsController(IResultService resultService)
        {
            _resultService = resultService;
        }

        [HttpGet("{id}/stage")]
        public async Task<int> GetById(int id)
        {
            return await _resultService.GetResultsByStageId(id);

        }

        [HttpGet("{stageId}/uitslag")]
        public async Task<IActionResult> GetEtappeUitslag(int stageId)
        {
            var uitslag = await _resultService.GetEtappeUitslag(stageId);
            return Ok(uitslag);
        }

        [HttpGet("top15/{eventId}")]
        public async Task<IActionResult> GetTop15(int eventId)
        {
            var top15 = await _resultService.GetResultsByEventId(eventId);
            return Ok(top15);
        }
    }
}
