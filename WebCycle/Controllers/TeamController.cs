using CycleManager.Services.Interfaces;
using Domain.Interfaces;
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
        public async Task<IEnumerable<Team>> GetAllTeams()
        {
            return await _teamService.GetAll();
        }

        [HttpGet("{id}", Name = "Get")]
        public async Task<Team> GetTeam(int id) 
        {
            return await _teamService.GetTeamById(id);
        }
    }
}
