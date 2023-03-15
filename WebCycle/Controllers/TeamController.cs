using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebCycle.Controllers
{
    [Route("/api/teams")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly ITeamRepository teamRepository;
        public TeamController(ITeamRepository teamRepository)
        {
            this.teamRepository = teamRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<Team>> GetAllTeams()
        {
            return await teamRepository.GetAll();
        }

        [HttpGet("{id}", Name = "Get")]
        public async Task<Team> GetTeam(int id) 
        {
            return await teamRepository.GetById(id);
        }
    }
}
