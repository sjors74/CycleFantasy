using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebCycle.Controllers
{
    [Route("/api/teams")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        public TeamController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IEnumerable<Team> GetAllTeams()
        {
            return unitOfWork.Team.GetAll();
        }

        [HttpGet("{id}", Name = "Get")]
        public Team GetTeam(int id) 
        {
            return unitOfWork.Team.GetById(id);
        }
    }
}
