using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebCycle.Controllers
{
    [Route("/api/teams")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;
        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Team>>> GetAllTeams()
        {
            var teams = await _teamService.GetAllTeams();
            return Ok(teams);
        }

        [HttpGet("{id}", Name = "Get")]
        public async Task<ActionResult<Team>> GetTeam(int id) 
        {
            var team = await _teamService.GetTeamById(id);
            if (team == null)
                return NotFound();

            return Ok(team);
        }
    }
}
